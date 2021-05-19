import com.hazelcast.client.HazelcastClient;
import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.map.IMap;

import java.io.IOException;
import java.io.InputStreamReader;
import java.io.LineNumberReader;
import java.util.ArrayList;
import java.util.List;

public class Client {
	// Reader for user input
	private LineNumberReader in = new LineNumberReader(new InputStreamReader(System.in));
	// Connection to the cluster
	private HazelcastInstance hazelcast;
	// The name of the user
	private String userName;
	// Do not keep any other state here - all data should be in the cluster

	/**
	 * Create a client for the specified user.
	 * @param userName user name used to identify the user
	 */
	public Client(String userName) {
		this.userName = userName;
		// Connect to the Hazelcast cluster
		// ClientConfig config = new ClientConfig();
		hazelcast = HazelcastClient.newHazelcastClient();
	}

	/**
	 * Disconnect from the Hazelcast cluster.
	 */
	public void disconnect() {
		// Disconnect from the Hazelcast cluster
		hazelcast.shutdown();
	}

	private String getSelectedDocumentName() {
		IMap<String, String> selectedMap = hazelcast.getMap("SelectedDocuments");
		String selected = selectedMap.getOrDefault(userName, null);
		if (selected == null) {
			System.out.println("No document selected");
		}
		return selected;
	}

	private void fetchDocument(String documentName) {
		IMap<String, Document> documentMap = hazelcast.getMap("Documents");
		IMap<String, String> selectedMap = hazelcast.getMap("SelectedDocuments");
		IMap<String, Integer> counterMap = hazelcast.getMap("AccessCounts");

		// Set the current selected document for the user
		selectedMap.set(userName, documentName);

		// Get the document (from the cache, or generated)
		Document document = documentMap.executeOnKey(documentName, new FetchDocumentProcessor());

		counterMap.executeOnKey(documentName, new IncrementAccessCounterProcessor());
		// Show the document content
		System.out.println("The document is:");
		System.out.println(document.getContent());
	}

	/**
	 * Read a name of a document,
	 * select it as the current document of the user
	 * and show the document content.
	 */
	private void showCommand() throws IOException {
		System.out.println("Enter document name:");
		String documentName = in.readLine();
		fetchDocument(documentName);
	}

	/**
	 * Show the next document in the list of favorites of the user.
	 * Select the next document, so that running this command repeatedly
	 * will cyclically show all favorite documents of the user.
	 */
	private void nextFavoriteCommand() {
		IMap<String, List<String>> favoritesMap = hazelcast.getMap("FavoriteLists");
		String selectedName = getSelectedDocumentName();
		String nextFavoriteName = favoritesMap.executeOnKey(userName, new GetNextFavoriteProcessor(selectedName));
		if (nextFavoriteName == null) {
			System.out.println("No favorite documents found.");
			return;
		}

		fetchDocument(nextFavoriteName);
	}

	/**
	 * Add the current selected document name to the list of favorite documents of the user.
	 * If the list already contains the document name, do nothing.
	 */
	private void addFavoriteCommand() {
		IMap<String, List<String>> favoritesMap = hazelcast.getMap("FavoriteLists");

		String selectedDocumentName = getSelectedDocumentName();
		if (selectedDocumentName == null) {
			return;
		}

		favoritesMap.executeOnKey(userName, new AddToFavoritesProcessor(selectedDocumentName.toString()));
		System.out.printf("Added %s to favorites%n", selectedDocumentName);
	}
	/**
	 * Remove the current selected document name from the list of favorite documents of the user.
	 * If the list does not contain the document name, do nothing.
	 */
	private void removeFavoriteCommand(){
		String selectedDocumentName = getSelectedDocumentName();
		if (selectedDocumentName == null) {
			return;
		}

		IMap<String, List<String>> favoritesMap = hazelcast.getMap("FavoriteLists");
		boolean removed = favoritesMap.executeOnKey(userName, new RemoveFromFavoritesProcessor(selectedDocumentName));
		if (removed) {
			System.out.printf("Removed %s from favorites%n", selectedDocumentName);
		} else {
			System.out.printf("Couldn't find %s in your favorites%n", selectedDocumentName);
		}
	}
	/**
	 * Add the current selected document name to the list of favorite documents of the user.
	 * If the list already contains the document name, do nothing.
	 */
	private void listFavoritesCommand() {
		// Get the list of favorite documents of the user
		IMap<String, List<String>> favoritesMap = hazelcast.getMap("FavoriteLists");
		List<String> favoriteList = favoritesMap.getOrDefault(userName, new ArrayList<>());
		// Print the list of favorite documents
		System.out.println("Your list of favorite documents:");
		for(String favoriteDocumentName: favoriteList)
			System.out.println(favoriteDocumentName);
	}

	/**
	 * Show the view count and comments of the current selected document.
	 */
	private void infoCommand(){
		IMap<String, Integer> viewCountMap = hazelcast.getMap("AccessCounts");
		IMap<String, List<String>> commentsMap = hazelcast.getMap("Comments");

		String selectedDocumentName = getSelectedDocumentName();
		if (selectedDocumentName == null) {
			return;
		}

		int viewCount = viewCountMap.getOrDefault(selectedDocumentName, 0);
		List<String> comments = commentsMap.getOrDefault(selectedDocumentName, new ArrayList<>());

		// Print the information
		System.out.printf("Info about %s:%n", selectedDocumentName);
		System.out.printf("Viewed %d times.%n", viewCount);
		System.out.printf("Comments (%d):%n", comments.size());
		for(String comment: comments)
			System.out.println(comment);
	}

	/**
	 * Add a comment about the current selected document.
	 */
	private void commentCommand() throws IOException{
		System.out.println("Enter comment text:");
		String commentText = in.readLine();

		IMap<String, List<String>> commentsMap = hazelcast.getMap("Comments");
		String selectedDocumentName = getSelectedDocumentName();
		if (selectedDocumentName == null) {
			return;
		}

		commentsMap.executeOnKey(selectedDocumentName, new AddCommentProcessor(commentText));
		// TODO: Add the comment to the list of comments of the selected document
		System.out.printf("Added a comment about %s.%n", selectedDocumentName);
	}

	/*
	 * Main interactive user loop
	 */
	public void run() throws IOException {
		loop:
		while (true) {
			System.out.println("\nAvailable commands (type and press enter):");
			System.out.println(" s - select and show document");
			System.out.println(" i - show document view count and comments");
			System.out.println(" c - add comment");
			System.out.println(" a - add to favorites");
			System.out.println(" r - remove from favorites");
			System.out.println(" n - show next favorite");
			System.out.println(" l - list all favorites");
			System.out.println(" q - quit");
			// read first character
			int c = in.read();
			// throw away rest of the buffered line
			while (in.ready())
				in.read();
			switch (c) {
				case 'q': // Quit the application
					break loop;
				case 's': // Select and show a document
					showCommand();
					break;
				case 'i': // Show view count and comments of the selected document
					infoCommand();
					break;
				case 'c': // Add a comment to the selected document
					commentCommand();
					break;
				case 'a': // Add the selected document to favorites
					addFavoriteCommand();
					break;
				case 'r': // Remove the selected document from favorites
					removeFavoriteCommand();
					break;
				case 'n': // Select and show the next document in the list of favorites
					nextFavoriteCommand();
					break;
				case 'l': // Show the list of favorite documents
					listFavoritesCommand();
					break;
				case '\n':
				default:
					break;
			}
		}
	}

	/*
	 * Main method, creates a client instance and runs its loop
	 */
	public static void main(String[] args) {
		if (args.length != 1) {
			System.err.println("Usage: ./client <userName>");
			return;
		}

		try {
			Client client = new Client(args[0]);
			try {
				client.run();
			}
			finally {
				System.out.println("Disconnecting...");
				client.disconnect();
			}
		}
		catch (Exception e){
			e.printStackTrace();
		}
	}

}
