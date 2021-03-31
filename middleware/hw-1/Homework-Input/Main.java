import java.net.MalformedURLException;
import java.rmi.Naming;
import java.rmi.NotBoundException;
import java.rmi.RemoteException;
import java.util.Random;

public class Main {
	// How many nodes and how many edges to create.
	private static int GRAPH_NODES;
	private static int GRAPH_EDGES;

	// How many searches to perform
	private static final int SEARCHES = 50;

	private static final Random random = new Random();

	/**
	 * Creates nodes of a graph.
	 * 
	 * @param howMany number of nodes
	 */
	public static Node[] createNodes(int howMany) {
		Node[] nodes = new Node[howMany];

		for (int i = 0; i < howMany; i++) {
			nodes[i] = new NodeImpl();
		}

		return nodes;
	}

	/**
	 * Creates a fully connected graph.
	 */
	public static void connectAllNodes(Node[] nodes) {
		for (int idxFrom = 0; idxFrom < nodes.length; idxFrom++) {
			for (int idxTo = idxFrom + 1; idxTo < nodes.length; idxTo++) {
				nodes[idxFrom].addNeighbor(nodes[idxTo]);
				nodes[idxTo].addNeighbor(nodes[idxFrom]);
			}
		}
	}

	/**
	 * Creates a randomly connected graph.
	 * 
	 * @param howMany number of edges
	 */
	public static void connectSomeNodes(int howMany, Node[] nodes) {
		for (int i = 0; i < howMany; i++) {
			final int idxFrom = random.nextInt(nodes.length);
			final int idxTo = random.nextInt(nodes.length);

			nodes[idxFrom].addNeighbor(nodes[idxTo]);
		}
	}

	/**
	 * Runs a quick measurement on the graph.
	 *
	 * @param howMany number of measurements
	 * @param nodes
	 */
	public static void searchBenchmark(int howMany, Node[] nodes, Searcher local, Searcher remote) throws RemoteException {
		// Display measurement header.
		System.out.printf("%7s %8s %13s %13s %13s%n", "Attempt", "Distance", "Time", "TTime", "RTime");
		for (int i = 0; i < howMany; i++) {
			// Select two random nodes.
			final int idxFrom = random.nextInt(nodes.length);
			final int idxTo = random.nextInt(nodes.length);

			// Calculate distance, measure operation time
			final long startTimeNs = System.nanoTime();
			final int distance = local.getDistance(nodes[idxFrom], nodes[idxTo]);
			final long durationNs = System.nanoTime() - startTimeNs;

			// Calculate transitive distance, measure operation time
			final long startTimeTransitiveNs = System.nanoTime();
			final int distanceTransitive = local.getDistanceTransitive(4, nodes[idxFrom], nodes[idxTo]);
			final long durationTransitiveNs = System.nanoTime() - startTimeTransitiveNs;

			// Remote searcher benchmark
			long durationRemoteNs = 0;
			if (remote != null) {
				final long startTimeRemoteNs = System.nanoTime();
				int distanceRemote = remote.getDistance(nodes[idxFrom], nodes[idxTo]);
				durationRemoteNs = System.nanoTime() - startTimeRemoteNs;
			}

			if (distance != distanceTransitive) {
				System.out.printf("Standard and transitive algorithms inconsistent (%d != %d)%n", distance,
						distanceTransitive);
			} else {
				// Print the measurement result.
				System.out.printf("%7d %8d %13d %13d %13d%n", i, distance, durationNs / 1000,
						durationTransitiveNs / 1000, durationRemoteNs / 1000);
			}
		}
	}

	public static void main(String[] args) throws RemoteException, MalformedURLException, NotBoundException {
		if (args.length < 2) {
			System.out.println("Number of nodes and edges expected as arguments");
			return;
		}
		GRAPH_NODES = Integer.parseInt(args[0]);
		GRAPH_EDGES = Integer.parseInt(args[1]);
		Searcher local = new SearcherImpl();
		Searcher remote = null;

		for (int i = 2; i < args.length; i++) {
			if (args[i].equals("remote-searcher")) {
				remote = (Searcher) Naming.lookup("//localhost/RemoteSearcher");
			}

		}

		Node[] nodes = createNodes(GRAPH_NODES);
		connectSomeNodes(GRAPH_EDGES, nodes);
		searchBenchmark(SEARCHES, nodes, local, remote);
	}
}
