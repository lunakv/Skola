import com.hazelcast.core.Offloadable;
import com.hazelcast.core.ReadOnly;
import com.hazelcast.map.EntryProcessor;
import java.util.List;
import java.util.Map;

public class GetNextFavoriteProcessor implements EntryProcessor<String, List<String>, String>, Offloadable, ReadOnly {
    private final String currentSelected;

    public GetNextFavoriteProcessor(String name) {
        currentSelected = name;
    }

    @Override
    public String process(Map.Entry<String, List<String>> entry) {
        System.out.println("Executing GetNextFavoriteProcessor...");
        List<String> favoritesList = entry.getValue();
        if (favoritesList == null || favoritesList.size() == 0) return null;

        int index = currentSelected == null ? -1 : favoritesList.indexOf(currentSelected);
        return favoritesList.get(index == favoritesList.size() - 1 ? 0 : index + 1);
    }

    @Override
    public String getExecutorName() {
        return OFFLOADABLE_EXECUTOR;
    }
}
