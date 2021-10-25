.section .data
.byte 10
.short 1000
.align 2
foo:
.long 100000
.quad 10000000000
.balign 4
bar:
.ascii "Foo, 42, 1 + 2 = 3, \x07\b\t\n\v\f\r\x1B\"\'\\ \xFF, \xfe\xca, \xde\xc0\xad\xde"
.balign 8
.string "Foo, 42, 1 + 2 = 3, \x07\b\t\n\v\f\r\x1B\"\'\\ \xFF, \xfe\xca, \xde\xc0\xad\xde"
.long something

.section .other
start:
.long foo
.balign 16
.global baz
baz:
.quad bar
.short 16
