smallList x = takeWhile ((< x). (^2)) [1..]
doubleList l = map (*2) l

sumDoubleSmallList n = sum (map (*2) (takeWhile ((< n) . (^2)) [1..]))
sumDoubleSmallList' n = sum (doubleList (smallList n))

goldbachList :: Int -> [Int]
goldbachList n
    | (n == 1) = 1:[]
    | ((mod n 2) == 0) = (n):(goldbachList (n `div `2))
    | otherwise = (n):(goldbachList (3*n + 1))

getGoldbachLists n = map goldbachList [1..n]
getListLengths l = map length l

getListWithLength :: Int -> (Int, [Int])
getListWithLength n = 
    let list = (goldbachList n) 
    in ((length list), list)

lengthsAndLists n = map getListWithLength [1..n]
longestList n = 
    let (_, list) = maximum (lengthsAndLists n)
    in list