<script src="ajax.js"></script>
<section>
    <h1>Shopping List</h1>
    <div id="error"><?php if (isset($error)) echo $error; ?></div>
    <table class="striped">
        <thead>
            <tr>
                <th>Item</th>
                <th>Amount</th>
                <th></th>
                <th></th>
            </tr>
        </thead>
        <tbody id="items">
            <?php
            foreach ($list as $row) {
            ?>
                <tr class="item">
                    <td class="name"><?= $row['name']?></td>
                    <td class="amount">
                        <p class="amount-label"><?= $row['amount']?></p>
                        <input type="hidden" class="amount-input" min="1">
                    </td>     
                    <td class="up-column">
                        <button class="blue up" value="<?= $row['name']?>">&#8645;</button>
                        <button class="up-placeholder">&#8645;</button>
                    </td>
                    <td class="buttons">
                        <div class="edit-buttons">
                            <button class="yellow edit" value="<?= $row['name']?>">Edit</button>
                            <button class="red delete" value="<?= $row['name']?>">Delete</button>
                        </div>
                        <div class="confirm-buttons" hidden>
                            <button class="green confirm" value="<?= $row['name']?>">OK</button>
                            <button class="red cancel">Cancel</button>
                        </div>
                    </td>
                </tr>
            <?php
            }
            ?>
        </tbody>
    </table>

    <h1>Add Item</h1>
    <form id="add-form" action="?action=add" method="POST">
        <fieldset id="add-form-input">
            <div id="name-container">
                <label>Item:</label><br>
                <input type="text" name="name" list="item-suggestions" required>
            </div>
            <div id="amount-container">
                <label>Amount:</label><br>
                <input type="number" name="amount" required min="1">
            </div>
        </fieldset>
        <div id="submit-container">
            <button id="submit" class="green" type="submit">Add</button>
        </div>
    </form>

    <?php if (isset($datalist)) { 
        echo '<datalist id="item-suggestions">';
        foreach ($datalist as $row) {
            echo '<option value="'. $row["name"] . '">';
        }
        echo '</datalist>';
    } ?>

</section>
