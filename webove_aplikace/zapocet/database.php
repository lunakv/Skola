<?php

class Database
{
    // database connection
    private $conn;

    public function __construct()
    {
        require __DIR__ . '/db_config.php';
        $this->conn = new mysqli($db_config['server'], $db_config['login'], $db_config['password'], $db_config['database']);
        if ($this->conn->connect_errno)
        {
            throw new Exception('Database connection error');
        }
    }

    // sets the amount of an item
    public function update($name, $amount)
    {
        if (!is_numeric($amount))
        throw new Exception('Inserted amount is not a number');
        $amount = intval($amount);
        
        $query = 'UPDATE list SET amount = ? WHERE item_id IN (' . $this->nameToIdQuery($name) . ') ';
        $this->query($query, "is", $amount, $name);
    }    

    // adds a new item to the list
    public function add($name, $amount)
    {
        if (!is_numeric($amount))
        throw new Exception('Inserted amount is not a number');
        $amount = intval($amount);

        // insert into item if it doesn't exist
        $id = $this->getItemId($name);
        if ($id === FALSE)
        {
            $id = $this->addItem($name);
        }

        if ($this->isInList($name))
        {
            throw new Exception('Item already in list.');
        }

        $query = 'INSERT INTO list (item_id, amount, position) VALUES (?,?,?)';
        $this->query($query, "iii", $id, $amount, $this->getNextPosition());
        return $this->conn->insert_id;
    }

    // delete item from list
    public function delete($name)
    {
        $query = 'DELETE FROM list WHERE item_id IN (' . $this->nameToIdQuery($name) . ') ';
        $position = $this->getPosition($name);
        $this->query($query, "s", $name);
        $this->shiftPositions($position);
    }

    // get assoc array of all rows in list
    public function getList()
    {
        $query = 'SELECT name, amount, position FROM list JOIN items ON (list.item_id = items.id) ORDER BY position';
        $res = $this->query($query);
        return $res->fetch_all(MYSQLI_ASSOC);
    }

    // get assoc array of all items
    public function getItems()
    {
        $query = 'SELECT name FROM items';
        $res = $this->query($query);
        return $res->fetch_all(MYSQLI_ASSOC);
    }

    public function close()
    {
        $this->conn->close();
    }

    function getPosition($name)
    {
        $query = 'SELECT position FROM list WHERE item_id IN (' . $this->nameToIdQuery($name) . ') ';
        $res = $this->query($query, "s", $name);
        return $res->fetch_assoc()['position'];
    }

    function moveUp($name)
    {
        $pos = $this->getPosition($name);
        if ($pos === 1)
            throw new Exception("First item cannot be moved up");

        $query = 'UPDATE list SET position = ? WHERE position = ?';
        $this->query($query, "ii", $pos, $pos-1);
        
        $query = 'UPDATE list SET position = ? WHERE item_id IN ('.$this->nameToIdQuery($name).')'; 
        $this->query($query, "is", $pos-1, $name);
        return $this->getPosition($name);
    }

    // shift positions of items above $position down by one (after deleting an item)
    function shiftPositions($position)
    {
        if (!is_numeric($position))
            throw new Exception("Specified position is not numeric.");
        $query = 'UPDATE list SET position = position - 1 WHERE position > ?';
        $this->query($query, "i", $position);
    }

    function addItem($name)
    {
        $query = 'INSERT INTO items (name) VALUES (?) ';
        $res = $this->query($query, "s", $name);
        return $this->conn->insert_id;
    }

    function getItemId($name)
    {
        $query = $this->nameToIdQuery($name);
        $res = $this->query($query, "s", $name);
        if ($res->num_rows == 0) return FALSE;
        return $res->fetch_assoc()['id'];
    }

    function isInList($name)
    {
        $query = 'SELECT * FROM list WHERE item_id IN (' . $this->nameToIdQuery($name) . ') ';
        $res = $this->query($query, "s", $name);
        return $res->num_rows > 0;
    }

    // get next free position in list (for adding new item)
    function getNextPosition()
    {
        $query = 'SELECT MAX(position) FROM list';
        $res = $this->query($query);
        $val = $res->fetch_array()[0];
        if (!is_numeric($val)) $val = 0;
        else $val = intval($val);

        return $val+1;
    }

    function nameToIdQuery($name)
    {
        return 'SELECT id FROM items WHERE name = ?';
    }

    // generic query sending method - binds parameters if specified,
    //      returns query result or throws an exception
    function query($query, $types = null, ...$params)
    {
        if ($types === null) {
            $res = $this->conn->query($query);
            if ($this->conn->errno !== 0)
                throw new Exception('Database error: '.$this->conn->error);
            return $res;
        } else {
            $stmt = $this->conn->prepare($query);
            $stmt->bind_param($types, ...$params);
            $stmt->execute();
            if ($stmt->errno !== 0)
                throw new Exception('Database error: '.$stmt->error);
            return $stmt->get_result();
        }
    }
}