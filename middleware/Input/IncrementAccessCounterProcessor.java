import com.hazelcast.map.EntryProcessor;

import java.util.Map;

public class IncrementAccessCounterProcessor implements EntryProcessor<String, Integer, Integer> {

    @Override
    public Integer process(Map.Entry<String, Integer> entry) {
        Integer value = entry.getValue();
        if (value == null) value = 0;

        entry.setValue(++value);
        return value;
    }
}
