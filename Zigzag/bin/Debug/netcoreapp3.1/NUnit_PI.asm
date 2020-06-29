section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate

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
mov rdi, rsi
call allocate
mov rsi, rax
mov rcx, rbx
sal rcx, 3
mov rbp, rdi
call allocate
mov rdi, rax
mov rcx, rbp
sal rcx, 3
mov r12, rbp
call allocate
mov rbp, rax
mov r13, r12
xor r12, r12
cmp r12, rbx
jge function_pidigits_L1
function_pidigits_L0:
mov qword [rsi+r12*8], 20
add r12, 1
cmp r12, rbx
jl function_pidigits_L0
function_pidigits_L1:
mov r14, rbx
xor rbx, rbx
cmp rbx, r13
jge function_pidigits_L3
function_pidigits_L2:
xor rax, rax
xor rcx, rcx
cmp rax, r14
jge function_pidigits_L5
function_pidigits_L4:
mov rdx, r14
sub rdx, rax
sub rdx, 1
mov r8, rdx
sal r8, 1
add r8, 1
mov r9, [rsi+rax*8]
add r9, rcx
mov qword [rsi+rax*8], r9
mov r9, rax
mov rax, [rsi+r9*8]
mov r10, rdx
xor rdx, rdx
idiv r8
mov rdx, rax
mov rax, [rsi+r9*8]
mov r11, rdx
xor rdx, rdx
idiv r8
mov qword [rdi+r9*8], rdx
imul r11, r10
add r9, 1
cmp r9, r14
mov rcx, r11
mov rax, r9
jl function_pidigits_L4
function_pidigits_L5:
mov rdx, r14
sub rdx, 1
mov r8, rax
mov rax, [rsi+rdx*8]
mov r9, rdx
xor rdx, rdx
mov r10, 10
idiv r10
mov qword [rbp+rbx*8], rax
mov rdx, r14
sub rdx, 1
mov r9, r14
sub r9, 1
mov rax, [rsi+r9*8]
mov r10, rdx
xor rdx, rdx
mov r11, 10
idiv r11
mov qword [rdi+r10*8], rdx
xor r12, r12
cmp r12, r14
jge function_pidigits_L7
function_pidigits_L6:
mov rdx, [rdi+r12*8]
imul rdx, 10
mov qword [rsi+r12*8], rdx
add r12, 1
cmp r12, r14
jl function_pidigits_L6
function_pidigits_L7:
add rbx, 1
cmp rbx, r13
jl function_pidigits_L2
function_pidigits_L3:
mov rcx, r13
sal rcx, 3
call allocate
mov rcx, r13
sub rcx, 1
xor rdx, rdx
test rcx, rcx
jl function_pidigits_L9
function_pidigits_L8:
add qword [rbp+rcx*8], rdx
mov rdx, rax
mov rax, [rbp+rcx*8]
mov r8, rdx
xor rdx, rdx
mov r9, 10
idiv r9
mov rdx, rax
mov rax, [rbp+rcx*8]
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
xor rax, rax
add rsp, 40
ret

function_length_of:
xor rax, rax
function_length_of_L0:
movzx rdx, byte [rcx+rax]
test dl, dl
jne function_length_of_L1
ret
function_length_of_L1:
add rax, 1
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