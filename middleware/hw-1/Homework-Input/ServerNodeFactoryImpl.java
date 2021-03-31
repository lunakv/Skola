import java.rmi.RemoteException;
import java.rmi.server.UnicastRemoteObject;

public class ServerNodeFactoryImpl extends UnicastRemoteObject implements NodeFactory {

    protected ServerNodeFactoryImpl() throws RemoteException {}

    @Override
    public Node createNode() throws RemoteException {
        return new ServerNodeImpl();
    }
}
