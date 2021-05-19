import com.hazelcast.map.EntryProcessor;

import java.util.List;
import java.util.Map;

public class RemoveFromFavoritesProcessor implements EntryProcessor<String, List<String>, Boolean> {
    private final String favoriteName;

    public RemoveFromFavoritesProcessor(String name) {
        favoriteName = name;
    }

    @Override
    public Boolean process(Map.Entry<String, List<String>> entry) {
        System.out.println("Executing RemoveFromFavoritesProcessor...");
        List<String> favoriteList = entry.getValue();
        if (favoriteList == null)
            return false;

        boolean removed = favoriteList.remove(favoriteName);
        entry.setValue(favoriteList);
        return removed;
    }
}
