import java.net.MalformedURLException;
import java.rmi.Naming;
import java.rmi.RemoteException;

public class Server {
    public static void main(String[] args) throws RemoteException, MalformedURLException {
        RemoteSearcherImpl obj = new RemoteSearcherImpl();
        Naming.rebind("//localhost/RemoteSearcher", obj);
    }
}
