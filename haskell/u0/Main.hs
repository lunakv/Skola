module Main where

import Data.Char (isAlpha)

main :: IO ()
main = do
  input <- getContents
  let percent = 100 * (vowelCount input) `div` (alphaCount input)
  putStrLn $ show percent ++ "%"

alphaCount :: String -> Int
alphaCount = length . filter isAlpha

vowelCount :: [Char] -> Int
vowelCount = length . filter (`elem` "aeiouy")