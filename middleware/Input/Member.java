import com.hazelcast.config.Config;
import com.hazelcast.config.FileSystemYamlConfig;
import com.hazelcast.core.Hazelcast;
import com.hazelcast.core.HazelcastInstance;

import java.io.FileNotFoundException;
import java.io.IOException;

public class Member {
    public static void main(String[] args) {
        try {
            Config config = new FileSystemYamlConfig("hazelcast.yaml");
            HazelcastInstance hazelcast = Hazelcast.newHazelcastInstance(config);
            String memberName = hazelcast.getName();

            try {
                System.out.println("Member name: " + memberName);
                System.out.println("Press enter to exit");
                System.in.read();
            } catch (IOException e) {
                e.printStackTrace();
            }

            hazelcast.shutdown();
        } catch (FileNotFoundException e) {
            e.printStackTrace();
        }
    }
}
