import java.io.IOException;
import java.io.InputStreamReader;
import java.io.LineNumberReader;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Random;

import javax.jms.*;

import org.apache.activemq.artemis.jms.client.ActiveMQConnectionFactory;

public class Client {
	
	/****	CONSTANTS	****/
	
	// name of the property specifying client's name
	public static final String CLIENT_NAME_PROPERTY = "clientName";

	// name of the topic for publishing offers
	public static final String OFFER_TOPIC = "Offers";

	public static final String SALE_QUEUE_SUFFIX = ";SaleQueue";

	// text message command to send offered goods
	public static final String PUBLISH_REQUEST_MSG = "REPUBLISH";

	// MapMessage key for name of sold goods
	public static final String GOODS_NAME_KEY = "goodName";

	// MapMessage key for price of sold item
	public static final String PRICE_KEY = "price";

	// MapMessage key for indicator of accepted sale
	public static final String SALE_TYPE_KEY = "saleType";

	// sale type "accepted"
	public static final int SALE_ACCEPTED = 1;

	// sale type "rejected by seller"
	public static final int SALE_SELLER_REJECTED = 2;

	// sale type "rejected by bank"
	public static final int SALE_BANK_REJECTED = 3;

	// sale type "confirmed"
	public static final int SALE_CONFIRMED = 4;

	// MapMessage key for message with reason to deny sale
	public static final String DENY_REASON_KEY = "denyReason";

	public static final int REPORT_TYPE_SALE_REQUEST = 2;
	
	/****	PRIVATE VARIABLES	****/
	
	// client's unique name
	private String clientName;

	// client's account number
	private int accountNumber;
	
	// offered goods, mapped by name
	private Map<String, Goods> offeredGoods = new HashMap<String, Goods>();
	
	// available goods, mapped by seller's name 
	private Map<String, List<Goods>> availableGoods = new HashMap<String, List<Goods>>();
	
	// reserved goods, mapped by name of the goods
	private Map<String, Goods> reservedGoods = new HashMap<String, Goods>();
	
	// buyer's names, mapped by their account numbers
	private Map<Integer, String> reserverAccounts = new HashMap<Integer, String>();
	
	// buyer's reply destinations, mapped by their names
	private Map<String, Destination> reserverDestinations= new HashMap<String, Destination>();
	
	// connection to the broker
	private Connection conn;
	
	// session for user-initiated synchronous messages
	private Session clientSession;

	// session for listening and reacting to asynchronous messages
	private Session eventSession;
	
	// sender for the clientSession
	private MessageProducer clientSender;
	
	// sender for the eventSession
	private MessageProducer eventSender;

	// receiver of synchronous replies
	private MessageConsumer replyReceiver;
	
	// topic to send and receiver offers
	private Topic offerTopic;
	
	// queue for sending messages to bank
	private Queue toBankQueue;
	
	// queue for receiving synchronous replies
	private Queue replyQueue;

	private Queue saleQueue;

	
	// reader of lines from stdin
	private LineNumberReader in = new LineNumberReader(new InputStreamReader(System.in));
	
	/****	PRIVATE METHODS	****/
	
	/*
	 * Constructor, stores clientName, connection and initializes maps
	 */
	private Client(String clientName, Connection conn) {
		this.clientName = clientName;
		this.conn = conn;
		
		// generate some goods
		generateGoods();
	}
	
	/*
	 * Generate goods items
	 */
	private void generateGoods() {
		Random rnd = new Random();
		for (int i = 0; i < 10; ++i) {
			String name = "";
			
			for (int j = 0; j < 4; ++j) {
				char c = (char) ('A' + rnd.nextInt('Z' - 'A'));
				name += c;
			}
			
			offeredGoods.put(name, new Goods(name, rnd.nextInt(10000)));
		}
	}
	
	/*
	 * Set up all JMS entities, get bank account, publish first goods offer 
	 */
	private void connect() throws JMSException {
		// create two sessions - one for synchronous and one for asynchronous processing
		clientSession = conn.createSession(false, Session.AUTO_ACKNOWLEDGE);
		eventSession = conn.createSession(false, Session.AUTO_ACKNOWLEDGE);
		
		// create (unbound) senders for the sessions
		clientSender = clientSession.createProducer(null);
		eventSender = eventSession.createProducer(null);
		
		// create queue for sending messages to bank
		toBankQueue = clientSession.createQueue(Bank.BANK_QUEUE);
		// create a temporary queue for receiving messages from bank
		Queue fromBankQueue = eventSession.createTemporaryQueue();

		// temporary receiver for the first reply from bank
		// note that although the receiver is created within a different session
		// than the queue, it is OK since the queue is used only within the
		// client session for the moment
		MessageConsumer tmpBankReceiver = clientSession.createConsumer(fromBankQueue);        
		
		// start processing messages
		conn.start();
		
		// request a bank account number
		Message msg = eventSession.createTextMessage(Bank.NEW_ACCOUNT_MSG);
		msg.setStringProperty(CLIENT_NAME_PROPERTY, clientName);
		// set ReplyTo that Bank will use to send me reply and later transfer reports
		msg.setJMSReplyTo(fromBankQueue);
		clientSender.send(toBankQueue, msg);
		
		// get reply from bank and store the account number
		TextMessage reply = (TextMessage) tmpBankReceiver.receive();
		accountNumber = Integer.parseInt(reply.getText());
		System.out.println("Account number: " + accountNumber);
		
		// close the temporary receiver
		tmpBankReceiver.close();
		
		// temporarily stop processing messages to finish initialization
		conn.stop();
		
		/* Processing bank reports */
		
		// create consumer of bank reports (from the fromBankQueue) on the event session
		MessageConsumer bankReceiver = eventSession.createConsumer(fromBankQueue);
		
		// set asynchronous listener for reports, using anonymous MessageListener
		// which just calls our designated method in its onMessage method
		bankReceiver.setMessageListener(new MessageListener() {
			@Override
			public void onMessage(Message msg) {
				try {
					processBankReport(msg);
				} catch (Exception e) {
					e.printStackTrace();
				}
			}
		});

		/* Step 1: Processing offers */

		// create a topic both for publishing and receiving offers
		offerTopic = eventSession.createTopic(OFFER_TOPIC);
		
		// create a consumer of offers from the topic using the event session
		MessageConsumer offerReceiver = eventSession.createConsumer(offerTopic);
		offerReceiver.setMessageListener(new MessageListener() {
			@Override
			public void onMessage(Message msg) {
				try {
					processOffer(msg);
				} catch (Exception e) {
					e.printStackTrace();
				}
			}
		});

		/* Step 2: Processing sale requests */
		
		// create a queue for receiving sale requests
		// note that Session's createTemporaryQueue() is not usable here, the queue must have a name
		// that others will be able to determine from clientName (such as clientName + "SaleQueue")
		Queue saleQueue = eventSession.createQueue(clientName + SALE_QUEUE_SUFFIX);
		// create consumer of sale requests on the event session
		MessageConsumer saleReceiver = eventSession.createConsumer(saleQueue);
		// set asynchronous listener for sale requests (see above how it can be done)
		// which should call processSale()
		saleReceiver.setMessageListener(new MessageListener() {
			@Override
			public void onMessage(Message message) {
				try {
					processSale(message);
				} catch (Exception e) {
					e.printStackTrace();
				}
			}
		});
		
		// create temporary queue for synchronous replies
		replyQueue = clientSession.createTemporaryQueue();
		
		// create synchronous receiver of the replies
		replyReceiver = clientSession.createConsumer(replyQueue);
		
		// restart message processing
		conn.start();
		
		// send list of offered goods
		publishGoodsList(clientSender, clientSession);

		// ask for offered goods from other clients
		clientSender.send(offerTopic, clientSession.createTextMessage(PUBLISH_REQUEST_MSG));
	}

	/*
	 * Publish a list of offered goods
	 * Parameter is an (unbound) sender that fits into current session
	 * Sometimes we publish the list on user's request, sometimes we react to an event
	 */
	private void publishGoodsList(MessageProducer sender, Session session) throws JMSException {
		// create a message (of appropriate type) holding the list of offered goods
		ObjectMessage msg = session.createObjectMessage(new ArrayList<>(offeredGoods.values()));
		msg.setStringProperty(CLIENT_NAME_PROPERTY, clientName);
		
		// send the message using the sender passed as parameter
		sender.send(offerTopic, msg);
	}
	
	/*
	 * Send empty offer and disconnect from the broker 
	 */
	private void disconnect() throws JMSException {
		// delete all offered goods
		offeredGoods.clear();
		
		// send the empty list to indicate client quit
		publishGoodsList(clientSender, clientSession);
		
		// close the connection to broker
		conn.close();
	}
	
	/*
	 * Print known goods that are offered by other clients
	 */
	private void list() {
		System.out.println("Available goods (name: price):");
		// iterate over sellers
		for (String sellerName : availableGoods.keySet()) {
			System.out.println("From " + sellerName);
			// iterate over goods offered by a seller
			for (Goods g : availableGoods.get(sellerName)) {
				System.out.println("  " + g);
			}
		}
	}

	/*
	 * Get account balance from bank
	 */
	private void getBalance() throws JMSException {
		// create request and send it to bank
		TextMessage msg = clientSession.createTextMessage(Bank.LIST_BALANCE_MSG);
		msg.setStringProperty(CLIENT_NAME_PROPERTY, clientName);
		msg.setJMSReplyTo(replyQueue);
		clientSender.send(toBankQueue, msg);

		// synchronously receive response from the bank;
		Message reply = replyReceiver.receive();
		if (!(reply instanceof MapMessage)) {
			System.out.println("Unknown message received:\n" + reply);
			return;
		}
		MapMessage mapMsg = (MapMessage) reply;

		int cmd = mapMsg.getInt(Bank.REPORT_TYPE_KEY);
		if (cmd == Bank.REPORT_TYPE_BALANCE) {
			int balance = mapMsg.getInt(Bank.BALANCE_KEY);
			System.out.println("Account balance: " + balance);
		} else {
			System.out.println("Unexpected report type received:\n" + reply);
		}
	}
	
	/*
	 * Main interactive user loop
	 */
	private void loop() throws IOException, JMSException {
		// first connect to broker and setup everything
		connect();
		
		loop:
		while (true) {
			System.out.println("\nAvailable commands (type and press enter):");
			System.out.println(" l - list available goods");
			System.out.println(" p - publish list of offered goods");
			System.out.println(" b - buy goods");
			System.out.println(" a - show account balance");
			System.out.println(" q - quit");
			// read first character
			int c = in.read();
			// throw away rest of the buffered line
			while (in.ready()) in.read();
			try {
				switch (c) {
					case 'q':
						disconnect();
						break loop;
					case 'a':
						getBalance();
						break;
					case 'b':
						buy();
						break;
					case 'l':
						list();
						break;
					case 'p':
						publishGoodsList(clientSender, clientSession);
						System.out.println("List of offers published");
						break;
					case '\n':
					default:
						break;
				}
			} catch (Exception e) {
				e.printStackTrace();
			}
		}
	}
	
	/*
	 * Perform buying of goods
	 */
	private void buy() throws IOException, JMSException {
		// get information from the user
		System.out.println("Enter seller name:");
		String sellerName = in.readLine();
		System.out.println("Enter goods name:");
		String goodsName = in.readLine();

		// check if the seller exists
		List<Goods> sellerGoods = availableGoods.get(sellerName);
		if (sellerGoods == null) {
			System.out.println("Seller does not exist: " + sellerName);
			return;
		}

		/* Step 1: send a message to the seller requesting the goods */
		
		// create local reference to the seller's queue
		Queue saleQueue = clientSession.createQueue(sellerName + SALE_QUEUE_SUFFIX);
		// create message requesting sale of the goods
		// includes: clientName, goodsName, accountNumber
		// also include reply destination that the other client will use to send reply (replyQueue)
		MapMessage msg = clientSession.createMapMessage();
		msg.setStringProperty(CLIENT_NAME_PROPERTY, clientName);
		msg.setString(GOODS_NAME_KEY, goodsName);
		msg.setInt(Bank.REPORT_SENDER_ACC_KEY, accountNumber);
		msg.setJMSReplyTo(replyQueue);
		// send the message (with clientSender)
		clientSender.send(saleQueue, msg);

		/* Step 2: get seller's response and process it */

		// receive the reply (synchronously, using replyReceiver)
		Message reply = replyReceiver.receive();
		if (!(reply instanceof MapMessage)) {
			System.out.println("Invalid message received:\n" + msg);
			return;
		}
		MapMessage mapReply = (MapMessage) reply;
		// parse the reply
		// distinguish between "sell denied" and "sell accepted" message
		// in case of "denied", report to user and return from this method
		if (mapReply.getInt(SALE_TYPE_KEY) != SALE_ACCEPTED) {
			System.out.println("Sale denied.");
			System.out.println(mapReply.getString(DENY_REASON_KEY));
			return;
		}

		// in case of "accepted"
		// - obtain seller's account number and price to pay
		int price = mapReply.getInt(PRICE_KEY);
		int sellerAccount = mapReply.getInt(Bank.REPORT_SENDER_ACC_KEY);

		/* Step 3: send message to bank requesting money transfer */
		
		// create message ordering the bank to send money to seller
		MapMessage bankMsg = clientSession.createMapMessage();
		bankMsg.setStringProperty(CLIENT_NAME_PROPERTY, clientName);
		bankMsg.setInt(Bank.ORDER_TYPE_KEY, Bank.ORDER_TYPE_SEND);
		bankMsg.setInt(Bank.ORDER_RECEIVER_ACC_KEY, sellerAccount);
		bankMsg.setInt(Bank.AMOUNT_KEY, price);
		
		System.out.println("Sending $" + price + " to account " + sellerAccount);
		
		// send message to bank
		clientSender.send(toBankQueue, bankMsg);

		/* Step 4: wait for seller's sale confirmation */
		
		// receive the confirmation, similar to Step 2
		Message replyMsg = replyReceiver.receive();
		if (!(replyMsg instanceof MapMessage) || !sellerName.equals(replyMsg.getStringProperty(CLIENT_NAME_PROPERTY))) {
			System.out.println("Invalid reply message received:\n" + replyMsg);
			return;
		}

		MapMessage saleConfirmation = (MapMessage) replyMsg;
		// parse message and verify it's confirmation message
		if (saleConfirmation.getInt(SALE_TYPE_KEY) == SALE_CONFIRMED) {
			System.out.println("Sale confirmed.");
		} else {
			System.out.println("Sale denied.");
			System.out.println(saleConfirmation.getString(DENY_REASON_KEY));
		}

		/* Step 5: send confirmation of sale to the bank */

		// unless the bank rejected the sale, in which case it's unnecessary
		if (saleConfirmation.getInt(SALE_TYPE_KEY) != SALE_BANK_REJECTED) {
			MapMessage bankConfirmation = clientSession.createMapMessage();
			bankConfirmation.setStringProperty(CLIENT_NAME_PROPERTY, clientName);
			if (saleConfirmation.getInt(SALE_TYPE_KEY) == SALE_CONFIRMED) {
				bankConfirmation.setInt(Bank.ORDER_TYPE_KEY, Bank.ORDER_TYPE_CONFIRM);
			} else {
				bankConfirmation.setInt(Bank.ORDER_TYPE_KEY, Bank.ORDER_TYPE_ROLLBACK);
			}
			clientSender.send(toBankQueue, bankConfirmation);
		}
	}
	
	/*
	 * Process a message in the offers topic
	 */
	private void processOffer(Message msg) throws JMSException {
		// ignore messages sent from myself
		String sender = msg.getStringProperty(CLIENT_NAME_PROPERTY);
		if (clientName.equals(sender)) return;

		if (msg instanceof TextMessage) {
			// TextMessage in offerTopic should mean new client asking for offers
			if (PUBLISH_REQUEST_MSG.equals(((TextMessage) msg).getText())) {
				publishGoodsList(eventSender, eventSession);
			}
			// we ignore other text messages

		} else if (msg instanceof ObjectMessage) {
			// ObjectMessage is an offer of goods from someone

			ArrayList<Goods> goods;
			// cast isn't checked because Java can't handle generics properly
			try {
				goods = (ArrayList<Goods>) ((ObjectMessage) msg).getObject();
			} catch (ClassCastException e) {
				// ignore messages that aren't a list of goods
				return;
			}

			// store the list into availableGoods (replacing any previous offer)
			// empty list means disconnecting client, remove it from availableGoods completely
			if (goods == null || goods.isEmpty()) {
				availableGoods.remove(sender);
			} else {
				availableGoods.put(sender, goods);
			}
		} else {
			System.out.println("Received unknown offer:\n" + msg);
		}
	}
	
	/*
	 * Process message requesting a sale
	 */
	private void processSale(Message msg) throws JMSException {
		/* Step 1: parse the message */
		// distinguish that it's the sale request message
		if (!(msg instanceof MapMessage)) {
			System.out.println("Received invalid sale request message:\n" + msg);
			return;
		}
		MapMessage mapMsg = (MapMessage) msg;

		// obtain buyer's name (buyerName), goods name (goodsName) , buyer's account number (buyerAccount)
		String buyerName = mapMsg.getStringProperty(CLIENT_NAME_PROPERTY);
		String goodsName = mapMsg.getString(GOODS_NAME_KEY);
		int buyerAccount = mapMsg.getInt(Bank.REPORT_SENDER_ACC_KEY);

		// also obtain reply destination (buyerDest)
		Destination buyerDest = mapMsg.getJMSReplyTo();
		/* Step 2: decide what to do and modify data structures accordingly */

		if (reserverAccounts.containsKey(buyerAccount)) {
			// a buyer can't request an item if they already have a transaction pending
			MapMessage reply = eventSession.createMapMessage();
			reply.setInt(SALE_TYPE_KEY, SALE_SELLER_REJECTED);
			reply.setString(DENY_REASON_KEY, "You already have a pending transaction.");
			eventSender.send(buyerDest, reply);
			return;
		}

		// check if we still offer this goods
		Goods goods = offeredGoods.get(goodsName);
		if (goods != null) {
			// if yes, we should remove it from offeredGoods and publish new list
			// also it's useful to create a list of "reserved goods" together with buyer's information
			// such as name, account number, reply destination
			offeredGoods.remove(goodsName);
			reservedGoods.put(buyerName, goods);
			reserverAccounts.put(buyerAccount, buyerName);
			reserverDestinations.put(buyerName, buyerDest);
			publishGoodsList(eventSender, eventSession);
		}
		/* Step 3: send reply message */
		
		// prepare reply message (accept or deny)
		// accept message includes: my account number (accountNumber), price (goods.price)
		MapMessage replyMessage = eventSession.createMapMessage();
		if (goods != null) {
			replyMessage.setInt(SALE_TYPE_KEY, SALE_ACCEPTED);
			replyMessage.setInt(PRICE_KEY, goods.price);
			replyMessage.setInt(Bank.REPORT_SENDER_ACC_KEY, accountNumber);
		} else {
			replyMessage.setInt(SALE_TYPE_KEY, SALE_SELLER_REJECTED);
			replyMessage.setString(DENY_REASON_KEY, "Requested goods not offered.");
		}
		// send reply to buyer
		eventSender.send(buyerDest, replyMessage);
	}
	
	/*
	 * Process message with (transfer) report from the bank
	 */
	private void processBankReport(Message msg) throws JMSException {
		/* Step 1: parse the message */
		
		// Bank reports are sent as MapMessage
		if (!(msg instanceof MapMessage)) {
			System.out.println("Received unknown message:\n: " + msg);
			return;
		}
		MapMessage mapMsg = (MapMessage) msg;

		// get buyer account number
		int buyerAccount = mapMsg.getInt(Bank.REPORT_SENDER_ACC_KEY);

		// match the sender account with sender
		String buyerName = reserverAccounts.get(buyerAccount);

		// get buyer reply destination
		Destination buyerDestination = reserverDestinations.get(buyerName);

		// match the reserved goods and remove reservation
		Goods g = reservedGoods.get(buyerName);

		// remove the reserved goods and buyer-related information
		reserverDestinations.remove(buyerName);
		reserverAccounts.remove(buyerAccount);
		reservedGoods.remove(buyerName);

		// get report number
		int cmd = mapMsg.getInt(Bank.REPORT_TYPE_KEY);
		if (cmd == Bank.REPORT_TYPE_RECEIVED) {
			// Bank approved money transfer from buyer

			// get account number of sender and the amount of money sent
			int amount = mapMsg.getInt(Bank.AMOUNT_KEY);

			/* Step 2: decide what to do and modify data structures accordingly */

			// create reply to buyer
			MapMessage reply = eventSession.createMapMessage();
			reply.setStringProperty(CLIENT_NAME_PROPERTY, clientName);


			if (g == null) {
				reply.setInt(SALE_TYPE_KEY, SALE_SELLER_REJECTED);
				reply.setString(DENY_REASON_KEY, "No goods were not reserved by this client.");
			} else if (amount >= g.price) {
				System.out.println("Received $" + amount + " from " + buyerName);
				// send confirmation message
				reply.setInt(SALE_TYPE_KEY, SALE_CONFIRMED);
			} else {
				// re-offer item and send rejection message
				offeredGoods.put(g.name, g);
				reply.setInt(SALE_TYPE_KEY, SALE_SELLER_REJECTED);
				reply.setString(DENY_REASON_KEY, "Amount transferred was lower than item price.");
			}
			// send reply to the buyer
			eventSender.send(buyerDestination, reply);

		} else if (cmd == Bank.REPORT_TYPE_DENIED) {
			// Bank rejected money transfer from buyer

			// re-offer item and send pass rejection to buyer
			offeredGoods.put(g.name, g);
			MapMessage reply = eventSession.createMapMessage();
			reply.setStringProperty(CLIENT_NAME_PROPERTY, clientName);
			reply.setInt(SALE_TYPE_KEY, SALE_BANK_REJECTED);
			reply.setString(DENY_REASON_KEY, mapMsg.getString(DENY_REASON_KEY));
			eventSender.send(buyerDestination, reply);
			publishGoodsList(eventSender, eventSession);
		} else {
			System.out.println("Received unknown MapMessage:\n: " + msg);
		}
	}
	
	/**** PUBLIC METHODS ****/
	
	/*
	 * Main method, creates client instance and runs its loop
	 */
	public static void main(String[] args) {

		if (args.length != 1) {
			System.err.println("Usage: ./client <clientName>");
			return;
		}
		
		// create connection to the broker.
		try (ActiveMQConnectionFactory connectionFactory = new ActiveMQConnectionFactory("tcp://localhost:61616");
				Connection connection = connectionFactory.createConnection()) {
			// create instance of the client
			Client client = new Client(args[0], connection);
			
			// perform client loop
			client.loop();
		}
		catch (Exception e) {
			e.printStackTrace();
		}
	}
}
