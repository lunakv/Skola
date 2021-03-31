import java.rmi.RemoteException;
import java.rmi.server.UnicastRemoteObject;
import java.util.Map;
import java.util.Set;

public class RemoteNodeImpl extends UnicastRemoteObject implements Node {
    private final NodeImpl nodeImpl = new NodeImpl();
    protected RemoteNodeImpl() throws RemoteException {}

    @Override
    public Set<Node> getNeighbors() {
        return nodeImpl.getNeighbors();
    }

    @Override
    public Map<Node, Integer> getTransitiveNeighbors(int distance) throws RemoteException {
        return nodeImpl.getTransitiveNeighbors(distance);
    }

    @Override
    public void addNeighbor(Node neighbor) {
        nodeImpl.addNeighbor(neighbor);
    }
}
