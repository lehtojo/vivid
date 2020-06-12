section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate
extern ExitProcess

function_pidigits:
push rbx
push rsi
push rdi
push rbp
push r12
push r13
push r14
sub rsp, 48
add rcx, 1
mov rdx, rcx
imul rdx, 10
mov rax, rdx
mov r8, rdx
xor rdx, rdx
mov r9, 3
idiv r9
add rax, 2
mov rdx, rax
sal rdx, 3
mov r8, rcx
mov rcx, rdx
mov rbx, rax
mov rsi, r8
call allocate
mov rdi, rax
mov rcx, rbx
sal rcx, 3
call allocate
mov rbp, rax
mov rcx, rsi
sal rcx, 3
call allocate
mov r12, rax
xor r13, r13
cmp r13, rbx
jge function_pidigits_L1
function_pidigits_L0:
mov qword [rdi+r13*8], 20
add r13, 1
cmp r13, rbx
jl function_pidigits_L0
function_pidigits_L1:
mov r14, rbx
xor rbx, rbx
cmp rbx, rsi
jge function_pidigits_L3
function_pidigits_L2:
xor rcx, rcx
xor rdx, rdx
cmp rcx, r14
jge function_pidigits_L5
function_pidigits_L4:
mov r8, r14
sub r8, rcx
sub r8, 1
mov r9, r8
sal r9, 1
add r9, 1
mov r10, [rdi+rcx*8]
add r10, rdx
mov qword [rdi+rcx*8], r10
mov rax, [rdi+rcx*8]
mov r10, rdx
xor rdx, rdx
idiv r9
mov rdx, rax
mov rax, [rdi+rcx*8]
mov r11, rdx
xor rdx, rdx
idiv r9
mov qword [rbp+rcx*8], rdx
imul r11, r8
add rcx, 1
cmp rcx, r14
mov rdx, r11
jl function_pidigits_L4
function_pidigits_L5:
mov r8, r14
sub r8, 1
mov rax, [rdi+r8*8]
mov r9, rdx
xor rdx, rdx
mov r10, 10
idiv r10
mov qword [r12+rbx*8], rax
mov rdx, r14
sub rdx, 1
mov r8, r14
sub r8, 1
mov rax, [rdi+r8*8]
mov r10, rdx
xor rdx, rdx
mov r11, 10
idiv r11
mov qword [rbp+r10*8], rdx
xor rcx, rcx
cmp rcx, r14
jge function_pidigits_L7
function_pidigits_L6:
mov rdx, [rbp+rcx*8]
imul rdx, 10
mov qword [rdi+rcx*8], rdx
add rcx, 1
cmp rcx, r14
jl function_pidigits_L6
function_pidigits_L7:
add rbx, 1
cmp rbx, rsi
mov r13, rcx
jl function_pidigits_L2
function_pidigits_L3:
mov rcx, rsi
sal rcx, 3
call allocate
mov rcx, rsi
sub rcx, 1
xor rdx, rdx
test rcx, rcx
jl function_pidigits_L9
function_pidigits_L8:
add qword [r12+rcx*8], rdx
mov rdx, rax
mov rax, [r12+rcx*8]
mov r8, rdx
xor rdx, rdx
mov r9, 10
idiv r9
mov rdx, rax
mov rax, [r12+rcx*8]
mov r9, rdx
xor rdx, rdx
mov r10, 10
idiv r10
mov r10, 48
add r10, rdx
mov byte [r8+rcx], r10b
sub rcx, 1
test rcx, rcx
mov rax, r8
mov rdx, r9
jge function_pidigits_L8
function_pidigits_L9:
add rsp, 48
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

function_run:
sub rsp, 40
mov rcx, 3141
call function_pidigits
mov rcx, rax
call function_print
xor rcx, rcx
call ExitProcess
add rsp, 40
ret

function_length_of:
xor rdx, rdx
function_length_of_L0:
movzx r8, byte [rcx+rdx]
test r8b, r8b
jne function_length_of_L1
mov rax, rdx
ret
function_length_of_L1:
add rdx, 1
jmp function_length_of_L0
ret

function_print:
push rbx
sub rsp, 48
mov rbx, rcx
call function_length_of
mov rcx, rbx
mov rdx, rax
call sys_print
add rsp, 48
pop rbx
ret

section .data