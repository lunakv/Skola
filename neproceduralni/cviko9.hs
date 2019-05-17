data Tree a = Leaf | Node (Tree a) a (Tree a)
    deriving (Show, Eq)

instance Foldable Tree where
    foldMap _ Leaf = mempty
    foldMap f (Node l x r) = foldMap f l <> f x <> foldMap f r

-- > let t = Node (Node Leaf 2 Leaf) 4 (Node Leaf 6 Leaf)
-- > foldr (+) 0 t
-- 12
--
-- > length t
-- 3
--
-- Pomocí 'foldMap' implementujte:
newtype Sum a = Sum { getSum :: a }
newtype Product a = Product { getProduct :: a }

instance (Num a) => Semigroup (Sum a) where
    Sum a <> Sum b = Sum (a + b)

instance (Num a) => Monoid (Sum a) where
    mempty = Sum 0

instance (Num a) => Semigroup (Product a) where
    Product a <> Product b = Product (a * b)

instance (Num a) => Monoid (Product a) where
    mempty = Product 1


length' :: (Foldable t) => t a -> Int
length' = getSum . foldMap (\_ -> Sum 1)
--   foldMap :: Monoid m => (a -> m) -> t a -> m
sum' :: (Foldable t, Num a) => t a -> a
sum' = getSum . foldMap Sum

product' :: (Foldable t, Num a) => t a -> a
product' = getProduct . foldMap Product

toList' :: (Foldable t) => t a -> [a]
toList' = foldMap (\x -> [x])