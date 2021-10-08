.section text

.export main
add rax, 1
ret

.export foo
cmp rcx, rdx
je foo_L0
add rcx, rdx
foo_L0:
call bar
ret

baz:
mov dword [rcx+8], edx
%goo
ret

.section data
.byte 1
.word 2
.dword 4
.qword 8
.string 'String'
.ascii 'ASCII'