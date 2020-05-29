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
mov rbx, rax
mov rsi, rcx
mov rdi, rdx
mov rcx, rbx
sal rcx, 3
mov rbp, rdi
call allocate
mov rdi, rax
mov rcx, rbx
sal rcx, 3
mov r12, rbp
call allocate
mov rbp, rax
mov rcx, rsi
sal rcx, 3
mov r13, r12
call allocate
mov r12, rax
mov r14, r13
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
xor rax, rax
xor rcx, rcx
cmp rcx, r14
jge function_pidigits_L5
function_pidigits_L4:
mov rdx, r14
sub rdx, rcx
sub rdx, 1
mov r8, rdx
sal r8, 1
add r8, 1
mov r9, [rdi+rcx*8]
add r9, rax
mov qword [rdi+rcx*8], r9
mov r9, rax
mov rax, [rdi+rcx*8]
mov r10, rdx
xor rdx, rdx
idiv r8
mov r11, rax
mov rax, [rdi+rcx*8]
mov r13, rdx
xor rdx, rdx
idiv r8
mov qword [rbp+rcx*8], rdx
imul r11, r10
add rcx, 1
mov rax, r11
cmp rcx, r14
jl function_pidigits_L4
function_pidigits_L5:
mov rdx, r14
sub rdx, 1
mov r8, rax
mov rax, [rdi+rdx*8]
mov r9, rdx
xor rdx, rdx
mov r10, 10
idiv r10
mov qword [r12+rbx*8], rax
mov r9, r14
sub r9, 1
mov r10, r14
sub r10, 1
mov rax, [rdi+r10*8]
mov r11, rdx
xor rdx, rdx
mov r13, 10
idiv r13
mov qword [rbp+r9*8], rdx
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
mov r13, rcx
cmp rbx, rsi
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
mov r9, rax
mov rax, [r12+rcx*8]
mov r10, rdx
xor rdx, rdx
mov r11, 10
idiv r11
mov r11, 48
add r11, rdx
mov byte [r8+rcx], r11b
sub rcx, 1
mov rax, r8
mov rdx, r9
test rcx, rcx
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
xor rax, rax
function_length_of_L0:
movzx rdx, byte [rcx+rax]
test rdx, rdx
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
mov rcx, rbx
call function_length_of
mov rcx, rbx
mov rdx, rax
call sys_print
add rsp, 48
pop rbx
ret

section .data