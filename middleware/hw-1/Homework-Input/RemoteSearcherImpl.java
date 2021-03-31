import java.rmi.RemoteException;
import java.rmi.server.UnicastRemoteObject;

public class RemoteSearcherImpl extends UnicastRemoteObject implements Searcher {
    private static final Searcher searcherImpl = new SearcherImpl();

    protected RemoteSearcherImpl() throws RemoteException { }

    @Override
    public int getDistance(Node from, Node to) throws RemoteException {
        return searcherImpl.getDistance(from, to);
    }

    @Override
    public int getDistanceTransitive(int neighborDistance, Node from, Node to) throws RemoteException {
        return searcherImpl.getDistanceTransitive(neighborDistance, from, to);
    }
}