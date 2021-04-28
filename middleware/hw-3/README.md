## Compilation
Make sure `setenv` points to your Artemis installation home, then run
```
./install
./make
```

## Usage
First, start the broker with `./activemq run`. Then start the bank with `./bank`. Once the broker and the bank are running, you can start any number of clients with `./client <name>`.

## Documentation
### Message types
The solution uses three different message types: `TextMessage`, `ObjectMessage`, and `MapMessage`.

`TextMessage` is used when sending simple commands from the client to the bank. The simple commands are request for new account or request to send current balance. Because these commands don't require any additional parameters, a simple TextMessage is sufficient for them.

`ObjectMessage` is used for publishing offered items. These items are sent as a collection of object values, so we send them as a serialized `ArrayList` inside an `ObjectMessage`. 

`MapMessage` is used for all other communication. These messages need to contain several differing values that are all of scalar types and need to be individually accessible, which makes them a perfect use case for `MapMessage`. Since we don't have the collections of values we're sending available in a preexisting object in these cases, creating separate key-value pairs is more desirable than constructing and serializing a new object for an `ObjectMessage`.

### Communication
#### Creating a new account
To create a new account, the client sends a request to the bank. The bank creates a new account with $20000 if the client doesn't already have one, and replies with an integer ID corresponding to that client's account.

#### Fetching account balance
The client sends a request to the bank. The bank replies with that account's current balance, or 0 if the client doesn't have an account.

#### Offering goods
To offer its list of goods, the client publishes it into the Offer topic as an ObjectMessage. All clients are subscribed to the topic and upon receiving such message update their list of other sellers' items accordingly. Whenever that list is changed, the client re-publishes it, updating its state for all other clients.

When a new client joins, it can send a TextMessage into the Offer topic that requests others to publish their lists. When another client receives that message, they will re-publish their list of items into the topic. This makes sure that a newly connected client starts with up-to-date information about other clients' state and doesn't have to wait for other events to broadcast it.

#### Buying items
First, the buyer sends a message requesting the sale to the seller's Sale queue. This request contains information about the buyer and the goods that they are requesting. 

If the seller has the requested item available, it removes it from its item list, reserves it for the buyer and sends an "accept" message to them. If the item isn't available, or if this buyer already has a pending transaction with this seller, it sensd a "reject" message instead. For the sake of protocol simplicity, each buyer is allowed to have only one pending transaction at any given time. Aside from this status, the seller also sends either the current item price and account number, or the reason for the rejection.

On a "reject" message, the buyer simply displays a message and ends the procedure. If the seller accepts. On an "accept" message, they send a message to the bank, requesting a transfer of funds from their account to the seller's.

Once the bank receives this message, it checks the buyer's account balance. If it is high enough to facilitate the sale, the bank subtracts the requested amount from that account. Notably though, it doesn't add that money into the seller's account yet. Instead, it remembers where and how much should that client add, and then sends a notification about the transfer to the seller. If it determines that the transaction cannot be completed, it doesn't remove any funds from the buyer and just sends a message to the seller.

Once the seller receives this message, it decides on a reply based on its contents. If the message says that the bank rejected the transfer, or if the seller decides that this transaction isn't satisfactory (for example because the amount is too low), the seller simply moves the item from reservation back to its offered goods and sends a rejection message to the buyer. If the bank and the seller both accept the transfer, the reserved item is removed from reservation and a confirmation message is sent to the buyer.

The buyer receives this response and displays a status message based on whether the sale was successful. Then it sends one last message back to the bank, either confirming or rejecting the sale. The bank then either assigns the escrowed money into the seller's account, or returns it back to the buyer's account.

### Protocol Reasoning
#### Buying and Selling
There are several reasons for why this method of buying/selling items was chosen. 
- It requires minimal changes to the existing message flow. Using this method, the last message (the confirmation from the buyer to the bank) is the only new message added to the protocol. All other messages were already present in the initial version and required only small modifications.
- It presents a very simple way to handle failure. By using the bank as an escrow that keeps the money until the transaction is fully confirmed, reverting back is as simple as returning the escrowed money to the buyer's account. If we added money directly to the seller's account, we might run into a situation where the seller spends all that money and then a failure is encountered, but the seller's ballance isn't high enough for a rollback. This way, that cannot happen.
- It handles bank communication cleanly. If the seller were to report back to the bank, they would need to do so from the event session, complicating the implementation. This way, the bank is always contacted by the buyer from the synchronous client session, and always responds to the seller. 
- All communication is done in the same sequence. Regardless of state, the messages pertaining to one transaction will always be sent in the same direction and order, with the only difference being a possible early termination. If, for example, the bank were to send a message to the buyer directly when they have insufficient funds, it would create a need for additional handling, where we'd have to differentiate between messages sent by the buyer and messages sent by the bank. Because the seller needs to know about that rejection anyway (to close the transaction), it's easier to simply have them relay the failure to the buyer.

Of course, the main drawback of this protocol is that it relies on honesty on part of the buyer. If the seller approves a sale, the buyer could then report it as rejected and get their money back from the bank. While this is an issue, we decided that any reasonably simple protocol will require at least one party to behave honestly, and securing the bank against fraud on both sides is a complex problem that's outside the scope of this assignment, and thus we decided to work under the assumption that no client will intentionally behave maliciously. By the same token, there isn't any authentication done on behalf of the clients during communication.

The implementation is written to handle common edge cases. For example, clients requesting non-existing goods, paying insufficient funds, or sending unknown types of messages are situations that it is equipped to handle. That said, it still operates on some assumptions. It expects, for example, that if a message is determined to be of the correct type, it will contain all of the required properties and they will all have the correct type. This assumption was deemed as a reasonable compromise between robustness and brevity.

While the client and the bank are written to not crash even in the case of an unexpected failure, an error in an unopportune place may leave the system in an inconsistent state. When such a failure is encountered, it's recommended to restart the system.
