
# keys
endofobject
object
 - this is start
 - repeating key => value
 - until encountering endofobjectkey
array
 - this is start
 - repeating json datas
 - until encountering endofobject
string
 - byte length (max length defined at top of file)
 - utf8 encoded
number
 - number size? (in case of lots of tiny numbers)
 - number (max size defined at top of file)
bool
 - 1 bit
null
 - just that