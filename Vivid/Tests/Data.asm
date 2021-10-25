.section data
.byte 10
.word 1000
.align 1
foo:
.dword 100000
.qword 10000000000
.align 2
bar:
.characters 'Foo, 42, 1 + 2 = 3, \a\b\t\n\v\f\r\e\"\'\\ \xFF, \ucafe, \Udeadc0de'
.align 3
.string 'Foo, 42, 1 + 2 = 3, \a\b\t\n\v\f\r\e\"\'\\ \xFF, \ucafe, \Udeadc0de'
.dword something

.section other
start:
.dword foo
.align 4
.export baz
baz:
.qword bar
.word baz - start