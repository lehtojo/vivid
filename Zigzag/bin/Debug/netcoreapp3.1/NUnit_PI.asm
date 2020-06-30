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
mov rbx, rcx
imul rbx, 10
mov rax, rbx
xor rdx, rdx
mov r8, 3
idiv r8
add rax, 2
mov rdx, rax
sal rdx, 3
mov r8, rcx
mov rcx, rdx
mov rbx, rax
mov rsi, r8
call allocate
mov rcx, rbx
sal rcx, 3
mov rdi, rax
call allocate
mov rcx, rsi
sal rcx, 3
mov rbp, rax
call allocate
xor rcx, rcx
cmp rcx, rbx
jge function_pidigits_L1
function_pidigits_L0:
mov qword [rdi+rcx*8], 20
add rcx, 1
cmp rcx, rbx
jl function_pidigits_L0
function_pidigits_L1:
xor rdx, rdx
cmp rdx, rsi
jge function_pidigits_L3
function_pidigits_L2:
xor r8, r8
xor r9, r9
cmp r8, rbx
jge function_pidigits_L5
function_pidigits_L4:
mov rcx, rbx
sub rcx, r8
sub rcx, 1
mov r10, rcx
sal r10, 1
add r10, 1
mov r11, [rdi+r8*8]
add r11, r9
mov qword [rdi+r8*8], r11
mov r11, rax
mov rax, [rdi+r8*8]
mov r12, rdx
xor rdx, rdx
idiv r10
mov rdx, rax
mov rax, [rdi+r8*8]
mov r13, rdx
xor rdx, rdx
idiv r10
mov qword [rbp+r8*8], rdx
imul r13, rcx
add r8, 1
cmp r8, rbx
mov r9, r13
mov rax, r11
mov rdx, r12
jl function_pidigits_L4
function_pidigits_L5:
mov rcx, rbx
sub rcx, 1
mov r10, rax
mov rax, [rdi+rcx*8]
mov r11, rdx
xor rdx, rdx
mov r12, 10
idiv r12
mov qword [r10+r11*8], rax
mov rcx, rbx
sub rcx, 1
mov rdx, rbx
sub rdx, 1
mov rax, [rdi+rdx*8]
mov r12, rdx
xor rdx, rdx
mov r13, 10
idiv r13
mov qword [rbp+rcx*8], rdx
xor r14, r14
cmp r14, rbx
jge function_pidigits_L7
function_pidigits_L6:
mov rcx, [rbp+r14*8]
imul rcx, 10
mov qword [rdi+r14*8], rcx
add r14, 1
cmp r14, rbx
jl function_pidigits_L6
function_pidigits_L7:
add r11, 1
cmp r11, rsi
mov rax, r10
mov rcx, r14
mov rdx, r11
jl function_pidigits_L2
function_pidigits_L3:
mov r8, rsi
sal r8, 3
mov r9, rcx
mov rcx, r8
mov r12, rax
mov r13, rdx
mov r14, r9
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