-- Definice Trie
-- Každá instance trie musí být prvek Root se seznamem potomků, z nichž
--      žádný rekurzivně neobsahuje prvek Root.
-- Trie ve špatném formátu mohou způsobit nesprávné / nedefinované chování!
-- Předpokládá se zde, že žádný klíč není prefixem žádného jiného klíče 
--      - vnitřní uzle nemohou držet hodnoty
data Trie k v = Leaf k v | Inner k [Trie k v] | Root [Trie k v]
    deriving (Show, Eq)

getKey :: (Ord k) => Trie k v -> Maybe k
getKey (Leaf k _) = Just k
getKey (Inner k _) = Just k
getKey (Root _) = Nothing

-- Ze seznamu synů vybere toho s odpovídajícím klíčem, existuje-li
getSon :: (Ord k) => k -> [Trie k v] -> Maybe (Trie k v)
getSon _ [] = Nothing
getSon k (x:xs)
    | getKey x == Just k = Just x
    | otherwise = getSon k xs

-- Vytvoří prázdnou trii
empty :: Trie k v
empty = Root []

-- Vytvoří trii obsahující jediný klíč
singleton :: [k] -> v -> Trie k v
singleton [] _ = undefined          -- klíč musí být neprázdný
singleton (x:xs) value = Root [singleton' (x:xs) value]

-- Vytváří vnitřek singletona
singleton' :: [k] -> v -> Trie k v
singleton' [x] value = Leaf x value
singleton' (x:xs) value = Inner x [singleton' xs value]
singleton' [] _ = undefined

 
-- Najde hodnotu daného klíče, existuje-li
-- První parametr si pamatuje část klíče, která mi ještě zbývá najít
find :: (Ord k) => [k] -> (Trie k v) -> Maybe v
find [] (Leaf _ v) = Just v     -- když jsem došel až do listu, stačí vrátit hodnotu
find (x:xs) (Inner _ sons) =    -- ve vnitřních listech se rekurzivně volám na syny
    find' xs (getSon x sons)
find (x:xs) (Root sons) =       
    find' xs (getSon x sons)
find _ _ = Nothing  -- prázdný klíč ve vnitřním uzlu či neprázdný v listu

-- Wrapper kolem find, který odfiltrovává neexistující syny
find' :: (Ord k) => [k] -> Maybe (Trie k v) -> Maybe v
find' _ Nothing = Nothing
find' x (Just y) = find x y

isJust :: Maybe x -> Bool
isJust (Just _) = True
isJust Nothing = False
isMaybe :: Maybe x -> Bool
isMaybe x = not (isJust x)

-- Určuje, zda je určitý klíč obsažen v trii
member :: (Ord k) => [k] -> Trie k v -> Bool
member ks (Root sons) = isJust (find ks (Root sons))
member _ _ = undefined

-- Vloží nový prvek do trie, nebo upraví existující prvek pomocí funkce
-- Ve druhém parametru opět suffix klíče, který zbývá přečíst
insertWith :: (Ord k) => (v -> v -> v) -> [k] -> v -> Trie k v -> Trie k v
insertWith f (k:ks) v (Root sons) 
--  neexistuje syn -> přidáme nový pseudosingleton
    | isMaybe (getSon k sons) = Root (singleton' (k:ks) v:sons)
--  jinak musíme najít daného syna a změnit ho
    | otherwise = Root (modSons [] f (k:ks) v sons)
insertWith f (k:ks) v (Inner x sons)
    | isMaybe (getSon k sons) = Inner x (singleton' (k:ks) v:sons)
    | otherwise = Inner x (modSons [] f (k:ks) v sons)
-- V listu stačí modifikovat hodnotu
insertWith f [] v (Leaf x y) = Leaf x (f v y)
insertWith _ _ _ _ = undefined

-- Najde správného syna ze seznamu a zmodifikuje ho podle zadané funkce
modSons :: (Ord k) => [Trie k v] -> (v -> v -> v) -> [k] -> v -> [Trie k v] -> [Trie k v]
modSons acc f (k:ks) v (s:ss)
    | getKey s == Just k = [insertWith f ks v s] ++ acc ++ ss
    | otherwise = modSons (s:acc) f (k:ks) v ss
modSons _ _ _ _ _= []

first :: a -> a -> a
first a _ = a

-- Vloží nový prvek do trie, nebo přepíše existující prvek
insert :: (Ord k) => [k] -> v -> Trie k v -> Trie k v
insert ks v (Root s) = insertWith (first) ks v (Root s)
insert _ _ _ = undefined


-- testovací trie opsaná z Wikipedie
test :: Trie Char Integer
test = Root [(Inner 't' [Leaf 'o' 7, (Inner 'e' [Leaf 'a' 3, Leaf 'd' 4, Leaf 'n' 12])]), Leaf 'A' 15, (Inner 'i' [Inner 'n' [Leaf 'n' 5]])]
