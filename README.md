# BpJson (Bit-Packed Json)

Note: This project is very much a work in progress and is a bit messy. I intent to clean it up in the near future

This project allows you to serialize json to bytes in an extremely size-efficient way. There are a few different strategies that it uses to accomplish this, and this project was slightly over-engineered so make the serialization strategies for different json types modular, meaning it is fairly easy to try different combinations of serialization strategies to see which ones perform the best. This library was designed to work with large json files but should work with smaller files as well

# Compression Rates

#### Average/Medium File (TestData/example4.json)

| Format           | Size (bytes)  | % of Original Size |
| ---------------- | ------------- | ------------------ |
| json (oneline)   | 263,844       | original           |
| bpjson           | 90,583        | 34.34%             |
| bpjson (gzipped) | 61,307        | 23.24%             |

#### Large File (TestData/example4.json)

| Format           | Size (bytes)  | % of Original Size |
| ---------------- | ------------- | ------------------ |
| json (oneline)   | 14,005,976    | original           |
| bpjson           | 3,644,121     | 26.02%             |
| bpjson (gzipped) | 1,609,857     | 11.49%             |

# Why?

I have another project that does a lot of caching of json files. I've lately been getting up to 6-10 GB of stored json before I clear the cache so I figured it would be nice if that json took up less space. I thought about how inefficient json was as a storage format, and I'd been doing some bit-packing relating activities at work so bpjson was born.

# Future Plans

- [x] Add GZipping
- [ ] Find better compression / serialization techniques.
- [ ] Clean up the library and sort into a couple projects (Core, Core.BitPacking, Cli, etc.)
- [ ] Add a tests project
- [ ] Implement in other languages
