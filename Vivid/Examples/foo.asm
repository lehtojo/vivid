.section text

.export main
main:
lea rax, [rdi+rsi]
add rax, qword [foo]
ret

.section data
foo:
.qword bar

bar:
.qword 42

.qword 0
.qword 0
.qword 0
.qword 0

baz:
.qword bar