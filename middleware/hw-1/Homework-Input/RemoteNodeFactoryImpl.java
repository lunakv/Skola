import java.rmi.RemoteException;
import java.rmi.server.UnicastRemoteObject;

public class RemoteNodeFactoryImpl extends UnicastRemoteObject implements NodeFactory {

    protected RemoteNodeFactoryImpl() throws RemoteException {}

    @Override
    public Node createNode() throws RemoteException {
        return new RemoteNodeImpl();
    }
}
