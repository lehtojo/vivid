.section text

.export main:
push rbx
push rbp
add rax, 1
pop rbx
pop rbp
ret

.export foo:
cmp rcx, rdx
je foo_L0
add rcx, rdx
foo_L0:
call bar
ret

baz:
mov dword [rcx+8], edx
add rax, qword [r8+10]
add ebx, dword [rax+rcx]
add cx, word [rax+rcx+1]
add cl, byte [rcx*2+rax]
add cl, byte [rcx*2+rax+1]
ret

diudiu:
syscall
ret

.section data
.byte 1
.word 2
.dword 4
.qword 8
.string 'String'
.ascii 'ASCII'