import com.hazelcast.map.EntryProcessor;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

public class AddCommentProcessor implements EntryProcessor<String, List<String>, Object> {
    private final String comment;

    public AddCommentProcessor(String comment) {
        this.comment = comment;
    }

    @Override
    public Object process(Map.Entry<String, List<String>> entry) {
        System.out.println("Executing CommentAddProcessor...");
        List<String> commentList = entry.getValue();
        if (commentList == null) {
            commentList = new ArrayList<>();
        }

        commentList.add(comment);
        entry.setValue(commentList);
        return null;
    }
}
