
# Packing Strategies

## Strings
- Simple
- Simple w/ MaxLength in header
- Mapping
- Simple w/ " terminators

## Object Keys
- Simple
- Mapping
- Hierarchical Mapping
- Simple w/ " terminators

## Integers
- Simple (long)
- MaxLength in header
- VariableInt
- Customized VariableInt optimized for this particular file
- As String

## Floats
- Simple (double)
- VariableInt + VariableUInt
- As String