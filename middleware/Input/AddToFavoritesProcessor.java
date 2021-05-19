import com.hazelcast.core.Offloadable;
import com.hazelcast.map.EntryProcessor;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

public class AddToFavoritesProcessor implements EntryProcessor<String, List<String>, Object>, Offloadable {
    private final String favoriteName;

    public AddToFavoritesProcessor(String name) {
        favoriteName = name;
    }

    @Override
    public Object process(Map.Entry<String, List<String>> entry) {
        System.out.println("Executing AddToFavoritesProcessor...");
        List<String> favoritesList = entry.getValue();
        if (favoritesList == null) {
            favoritesList = new ArrayList<>();
        }

        favoritesList.add(favoriteName);
        entry.setValue(favoritesList);
        return null;
    }

    @Override
    public String getExecutorName() {
        return "hz:offloadable";
    }
}
