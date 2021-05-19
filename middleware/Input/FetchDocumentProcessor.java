import com.hazelcast.core.Offloadable;
import com.hazelcast.map.EntryProcessor;

import java.util.Map;

public class FetchDocumentProcessor implements EntryProcessor<String, Document, Document>, Offloadable {

    @Override
    public Document process(Map.Entry<String, Document> entry) {
        Document value = entry.getValue();
        if (value == null) {
            value = DocumentGenerator.generateDocument(entry.getKey());
            entry.setValue(value);
        }

        return value;
    }

    @Override
    public String getExecutorName() {
        return OFFLOADABLE_EXECUTOR;
    }
}
