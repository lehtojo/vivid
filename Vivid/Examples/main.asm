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

primitives:
movzx ecx, dx
movsx ecx, dx
mov rcx, rdx
add rcx, rdx
sub rcx, rdx
imul rcx, rdx
and rcx, rdx
or rcx, rdx
xor rcx, rdx
cmp rcx, rdx
addsd xmm1, xmm2
subsd xmm1, xmm2
mulsd xmm1, xmm2
divsd xmm1, xmm2
movsd xmm1, xmm2
cvtsi2sd xmm1, rdx
cvttsd2si rcx, xmm2
test rcx, rdx
sqrtsd xmm1, xmm2
xchg rcx, rdx

movzx ecx, word [rdx]
movsx ecx, word [rdx]
mov rcx, qword [rdx]
add rcx, qword [rdx]
sub rcx, qword [rdx]
imul rcx, qword [rdx]
and rcx, qword [rdx]
or rcx, qword [rdx]
xor rcx, qword [rdx]
cmp rcx, qword [rdx]
addsd xmm1, qword [rdx]
subsd xmm1, qword [rdx]
mulsd xmm1, qword [rdx]
divsd xmm1, qword [rdx]
movsd xmm1, qword [rdx]
cvtsi2sd xmm1, qword [rdx]
cvttsd2si rcx, qword [rdx]
xchg rcx, qword [rdx]

mov qword [rdx], rcx
add qword [rdx], rcx
sub qword [rdx], rcx
and qword [rdx], rcx
or qword [rdx], rcx
xor qword [rdx], rcx
cmp qword [rdx], rcx
movsd qword [rdx], xmm1
xchg qword [rdx], rcx

diudiu:
syscall
ret

.section data
.byte 1
.word 2
.dword 4
.qword 8
.string 'String'
.characters 'ASCII'