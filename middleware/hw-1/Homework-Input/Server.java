import java.net.MalformedURLException;
import java.rmi.Naming;
import java.rmi.RemoteException;

public class Server {
    public static void main(String[] args) throws RemoteException, MalformedURLException {
        Searcher searcher = new ServerSearcherImpl();
        NodeFactory factory = new ServerNodeFactoryImpl();
        Naming.rebind("//localhost/RemoteSearcher", searcher);
        Naming.rebind("//localhost/RemoteNodeFactory", factory);
    }
}
