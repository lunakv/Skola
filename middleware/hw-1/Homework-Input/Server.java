import java.net.MalformedURLException;
import java.rmi.Naming;
import java.rmi.RemoteException;

public class Server {
    public static void main(String[] args) throws RemoteException, MalformedURLException {
        RemoteSearcherImpl searcher = new RemoteSearcherImpl();
        RemoteNodeFactoryImpl factory = new RemoteNodeFactoryImpl();
        Naming.rebind("//localhost/RemoteSearcher", searcher);
        Naming.rebind("//localhost/RemoteNodeFactory", factory);
    }
}
