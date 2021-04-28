import java.util.HashMap;
import java.util.Map;

import javax.jms.*;

import org.apache.activemq.artemis.jms.client.ActiveMQConnectionFactory;

public class Bank implements MessageListener {
	
	/**** PUBLIC CONSTANTS ****/

	// text message command open new account
	public static final String NEW_ACCOUNT_MSG = "NEW_ACCOUNT";

	// text message command to list account balance
	public static final String LIST_BALANCE_MSG = "LIST_BALANCE";
	
	// MapMessage key for order type 
	public static final String ORDER_TYPE_KEY = "orderType";

	// order type "send money"
	public static final int ORDER_TYPE_SEND = 1;

	// order type "confirm money transfer"
	public static final int ORDER_TYPE_CONFIRM = 2;

	// order type "rollback money transfer"
	public static final int ORDER_TYPE_ROLLBACK = 3;
	
	// MapMessage key for receiver's account number
	public static final String ORDER_RECEIVER_ACC_KEY = "receiverAccount";
	
	// MapMessage key for amount of money transfered
	public static final String AMOUNT_KEY = "amount";
	
	// name of the queue for sending messages to Bank
	public static final String BANK_QUEUE = "BankQueue";

	// MapMessage key for report type
	public static final String REPORT_TYPE_KEY = "reportType";

	// MapMessage key for account balance
	public static final String BALANCE_KEY = "balance";

	// report type "received money"
	public static final int REPORT_TYPE_RECEIVED = 1;

	// report type "sale denied"
	public static final int REPORT_TYPE_DENIED = 2;

	// report type "account balance"
	public static final int REPORT_TYPE_BALANCE = 4;
	
	// MapMessage key for sender's account
	public static final String REPORT_SENDER_ACC_KEY = "senderAccount";
	
	/**** PRIVATE VARIABLES ****/

	// connection to broker
	private Connection conn;
	
	// session for asynchronous event messages
	private Session bankSession;
	
	// sender of (reply) messages, not bound to any destination
	private MessageProducer bankSender;

	// receiver of event messages
	private MessageConsumer bankReceiver;
	
	// Queue of incoming messages
	private Queue toBankQueue;
	
	// last assigned account number
	private int lastAccount  = 1000000;
	
	// map client names to client account numbers
	private Map<String, Integer> clientAccounts = new HashMap<String, Integer>();
	
	// map client account numbers to client names
	private Map<Integer, String> accountsClients = new HashMap<Integer, String>();
	
	// map client names to client report destinations
	private Map<String, Destination> clientDestinations = new HashMap<String, Destination>();

	// map client accounts to current balances
	private Map<Integer, Integer> accountBalances = new HashMap<>();

	// map accounts to pending transaction amounts
	private Map<Integer, Integer> escrowAmounts = new HashMap<>();

	// map client names to pending transaction destination clients
	private Map<Integer, Integer> escrowRecepients = new HashMap<>();

	
	/**** PRIVATE METHODS ****/
	
	/*
	 * Constructor, stores broker connection and initializes maps
	 */
	private Bank(Connection conn) {
		this.conn = conn;
	}
	
	/*
	 * Initialize messaging structures, start listening for messages
	 */
	private void init() throws JMSException {
		// create a non-transacted, auto acknowledged session
		bankSession = conn.createSession(false, Session.AUTO_ACKNOWLEDGE);
		
		// create queue for incoming messages
		toBankQueue = bankSession.createQueue(BANK_QUEUE);
		
		// create consumer of incoming messages
		bankReceiver = bankSession.createConsumer(toBankQueue);
		
		// receive messages asynchronously, using this object's onMessage()
		bankReceiver.setMessageListener(this);
		
		// create producer of messages, not bound to any destination
		bankSender = bankSession.createProducer(null);
		
		// start processing incoming messages
		conn.start();
	}
	
	/*
	 * Handle text messages
	 */
	private void processTextMessage(TextMessage txtMsg) throws JMSException {
		String clientName = txtMsg.getStringProperty(Client.CLIENT_NAME_PROPERTY);
		Destination replyDest = txtMsg.getJMSReplyTo();
		// is it a NEW ACCOUNT message?
		if (NEW_ACCOUNT_MSG.equals(txtMsg.getText())) {
			// store client's reply destination for future transfer reports
			clientDestinations.put(clientName, replyDest);
			int accountNumber;
			// either assign new account number or return already known number
			if (clientAccounts.get(clientName) != null) {
				accountNumber = clientAccounts.get(clientName);
			} else {
				accountNumber = lastAccount++;
				// also store the newly assigned number
				clientAccounts.put(clientName, accountNumber);
				accountsClients.put(accountNumber, clientName);
				accountBalances.put(accountNumber, 20000);
			}

			System.out.println("Connected client " + clientName + " with account " + accountNumber);

			// create reply TextMessage with the account number 
			TextMessage reply = bankSession.createTextMessage(String.valueOf(accountNumber));
			// send the reply to the provided reply destination
			bankSender.send(replyDest, reply);
		} else if (LIST_BALANCE_MSG.equals(txtMsg.getText())) {
			// is it a LIST BALANCE message?

			Integer accountNumber = clientAccounts.get(clientName);
			Integer balance = accountBalances.get(accountNumber);
			if (balance == null) balance = 0;
			MapMessage reply = bankSession.createMapMessage();
			reply.setInt(REPORT_TYPE_KEY, REPORT_TYPE_BALANCE);
			reply.setInt(BALANCE_KEY, balance);
			bankSender.send(replyDest, reply);
		} else {
			System.out.println("Received unknown text message: " + txtMsg.getText());
			System.out.println("Full message info:\n" + txtMsg);
		}
	}
	
	/*
	 * Handle map messages
	 */
	private void processMapMessage(MapMessage mapMsg) throws JMSException {
		// get the order type number
		int order = mapMsg.getInt(ORDER_TYPE_KEY);
		// get client's name
		String clientName = mapMsg.getStringProperty(Client.CLIENT_NAME_PROPERTY);
		// find client's account number
		Integer clientAccount = clientAccounts.get(clientName);
		// ignore messages from clients without accounts
		if (clientAccount == null) {
			System.out.println("Message from non-existing client " + clientName);
			return;
		}

		// process order to transfer money
		if (order == ORDER_TYPE_SEND) {
			// get receiver account number
			int destAccount = mapMsg.getInt(ORDER_RECEIVER_ACC_KEY);

			// find receiving client's name
			String destName = accountsClients.get(destAccount);

			// find receiving client's report message destination
			Destination dest = clientDestinations.get(destName);

			// get amount of money being transferred
			int amount = mapMsg.getInt(AMOUNT_KEY);

			// check if sender has enough money
			int senderBalance = accountBalances.get(clientAccount);

			// if they do, decrease their balance and send a successful message
			if (amount <= senderBalance) {
				System.out.println("Transferring $" + amount + " from account " + clientAccount + " to account " + destAccount);
				accountBalances.put(clientAccount, senderBalance - amount);
				escrowAmounts.put(clientAccount, amount);
				escrowRecepients.put(clientAccount, destAccount);

				// create report message for the receiving client
				MapMessage reportMsg = bankSession.createMapMessage();

				// set report type to "you received money"
				reportMsg.setInt(REPORT_TYPE_KEY, REPORT_TYPE_RECEIVED);

				// set sender's account number
				reportMsg.setInt(REPORT_SENDER_ACC_KEY, clientAccount);

				// set money of amount transfered
				reportMsg.setInt(AMOUNT_KEY, amount);

				// send report to receiver client's destination
				bankSender.send(dest, reportMsg);
			} else {
				System.out.println("Insufficient balance in account " + clientAccount + ": " + amount + " > " + senderBalance);
				MapMessage reportMsg = bankSession.createMapMessage();
				reportMsg.setInt(REPORT_TYPE_KEY, REPORT_TYPE_DENIED);
				reportMsg.setString(Client.DENY_REASON_KEY, "Insufficient account balance.");
				reportMsg.setInt(REPORT_SENDER_ACC_KEY, clientAccount);
				bankSender.send(dest, reportMsg);
			}
		} else if (order == ORDER_TYPE_CONFIRM || order == ORDER_TYPE_ROLLBACK) {
			// Process order to finalize a transaction

			Integer recipientAccount = escrowRecepients.remove(clientAccount);
			Integer transactionAmount = escrowAmounts.remove(clientAccount);
			// ignore confirmation for non-existent transaction
			if (recipientAccount == null || transactionAmount == null) {
				System.out.println("Unwarranted finalize message sent from account " + clientAccount);
				return;
			}

			if (order == ORDER_TYPE_CONFIRM) {
				System.out.println("Confirmed payment from " + clientAccount);
				Integer balance = accountBalances.get(recipientAccount);
				accountBalances.put(recipientAccount, balance + transactionAmount);
			} else {
				System.out.println("Rollback payment from " + clientAccount);
				Integer balance = accountBalances.get(clientAccount);
				accountBalances.put(clientAccount, balance + transactionAmount);
			}
		} else {
			System.out.println("Received unknown MapMessage:\n" + mapMsg);
		}
	}
	
	/**** PUBLIC METHODS ****/
	
	/*
	 * React to asynchronously received message
	 */
	@Override
	public void onMessage(Message msg) {
		// distinguish type of message and call appropriate handler
		try {
			if (msg instanceof TextMessage) {
				processTextMessage((TextMessage) msg);
			} else if (msg instanceof MapMessage) {
				processMapMessage((MapMessage) msg);
			} else {
				System.out.println("Received unknown message:\n: " + msg);
			}
		} catch (Exception e) {
			e.printStackTrace();
		}
	}
	
	/*
	 * Main method, create connection to broker and a Bank instance
	 */
	public static void main(String[] args) {
		// create connection to the broker.
		
		try (ActiveMQConnectionFactory connectionFactory = new ActiveMQConnectionFactory("tcp://localhost:61616");
				Connection connection = connectionFactory.createConnection()) {
			// create a bank instance
			Bank bank = new Bank(connection);
			// initialize bank's messaging
			bank.init();
			
			// bank now listens to asynchronous messages on another thread
			// wait for user before quit
			System.out.println("Bank running. Press enter to quit");
				
			System.in.read();
				
			System.out.println("Stopping...");
		}
		catch (Exception e) {
			e.printStackTrace();
		}
	}
}
