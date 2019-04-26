-- 1a) RLE encoding
rleEncode :: Eq a => [a] -> [(Int, a)]
rleEncode [] = []
rleEncode (x:xs)= rleEncode' (1,x) xs

-- remembers the last entry in a special parameter
rleEncode' :: Eq a => (Int, a) -> [a] -> [(Int, a)]
rleEncode' x [] = x:[]
rleEncode' (c,l) (x:xs) 
    | (l == x) = rleEncode' (c+1,l) xs
    | otherwise = (c,l):(rleEncode' (1,x) xs)

-- 1b) RLE decoding
rleDecode :: [(Int, a)] -> [a]
rleDecode [] = []
rleDecode ((count, char):xs) 
    | count == 0 = rleDecode xs
    | otherwise = char:rleDecode ((count-1, char):xs)

-- 2) Prime number generator
primes :: [Integer]
primes = 2:3:(filter isPrime [5,7..])

isPrime :: Integer -> Bool
isPrime x = [] == filter ((== 0) . (mod x)) (takeWhile ((<= x) . (^2)) [2..])

-- 3) Merge sort
mergeWith :: (a -> a -> Bool) -> [a] -> [a] -> [a]
mergeWith _ [] [] = []
mergeWith _ [] x = x
mergeWith _ x [] = x
mergeWith p (x:xs) (y:ys) 
    | p x y = x:(mergeWith p xs (y:ys))
    | otherwise = y:(mergeWith p (x:xs) ys)

sortWith :: (a -> a -> Bool) -> [a] -> [a]
sortWith _ [] = []
sortWith _ (x:[]) = [x] 
sortWith p list = 
    let (b, e) = splitAt (length list `div` 2) list
    in mergeWith p (sortWith p b) (sortWith p e)