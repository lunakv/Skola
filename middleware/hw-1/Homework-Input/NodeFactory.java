import java.rmi.Remote;
import java.rmi.RemoteException;

public interface NodeFactory extends Remote {
    Node createNode() throws RemoteException;
}
