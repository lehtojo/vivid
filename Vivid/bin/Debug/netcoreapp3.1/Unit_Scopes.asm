section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh
extern _V14large_functionv

global _V27scopes_nested_if_statementsxxxxxxxx_rx
export _V27scopes_nested_if_statementsxxxxxxxx_rx
_V27scopes_nested_if_statementsxxxxxxxx_rx:
push rbx
push rsi
push rdi
push rbp
push r12
push r13
push r14
push r15
sub rsp, 40
mov rbx, rcx
sal rbx, 1
lea rsi, [rdx*2+rdx]
lea rdi, [r8*4+r8]
mov rax, [rsp+144]
test rcx, rcx
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L1
test r8, r8
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L3
mov rbp, rax
mov r12, rcx
mov r13, rdx
mov r14, r8
mov r15, r9
call _V14large_functionv
mov rcx, r12
mov rdx, r13
mov r8, r14
mov r9, r15
mov rax, rbp
_V27scopes_nested_if_statementsxxxxxxxx_rx_L3:
mov rbp, rax
mov r12, rcx
mov r13, rdx
mov r14, r8
mov r15, r9
call _V14large_functionv
mov rcx, r12
mov rdx, r13
mov r8, r14
mov r9, r15
mov rax, rbp
jmp _V27scopes_nested_if_statementsxxxxxxxx_rx_L0
_V27scopes_nested_if_statementsxxxxxxxx_rx_L1:
test rdx, rdx
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L5
test r9, r9
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L7
mov rbp, rax
mov r12, rcx
mov r13, rdx
mov r14, r8
mov r15, r9
call _V14large_functionv
mov rcx, r12
mov rdx, r13
mov r8, r14
mov r9, r15
mov rax, rbp
_V27scopes_nested_if_statementsxxxxxxxx_rx_L7:
mov rbp, rax
mov r12, rcx
mov r13, rdx
mov r14, r8
mov r15, r9
call _V14large_functionv
mov rcx, r12
mov rdx, r13
mov r8, r14
mov r9, r15
mov rax, rbp
jmp _V27scopes_nested_if_statementsxxxxxxxx_rx_L0
_V27scopes_nested_if_statementsxxxxxxxx_rx_L5:
test rax, rax
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L9
mov rbp, rax
mov r12, rcx
mov r13, rdx
mov r14, r8
mov r15, r9
call _V14large_functionv
mov rcx, r12
mov rdx, r13
mov r8, r14
mov r9, r15
mov rax, rbp
_V27scopes_nested_if_statementsxxxxxxxx_rx_L9:
mov rbp, rax
mov r12, rcx
mov r13, rdx
mov r14, r8
mov r15, r9
call _V14large_functionv
mov rcx, r12
mov rdx, r13
mov r8, r14
mov r9, r15
mov rax, rbp
_V27scopes_nested_if_statementsxxxxxxxx_rx_L0:
add rcx, rdx
add rcx, r8
add rcx, r9
add rcx, rax
add rcx, [rsp+152]
add rcx, [rsp+160]
add rcx, [rsp+168]
imul rcx, rbx
imul rcx, rsi
imul rcx, rdi
mov rax, rcx
add rsp, 40
pop r15
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

global _V18scopes_single_loopxxxxxxxx_rx
export _V18scopes_single_loopxxxxxxxx_rx
_V18scopes_single_loopxxxxxxxx_rx:
push rbx
push rsi
push rdi
push rbp
push r12
push r13
push r14
push r15
sub rsp, 40
mov rbx, rcx
sal rbx, 1
lea rsi, [rdx*2+rdx]
lea rdi, [r8*4+r8]
xor rax, rax
mov r10, [rsp+168]
cmp rax, r10
jge _V18scopes_single_loopxxxxxxxx_rx_L1
_V18scopes_single_loopxxxxxxxx_rx_L0:
mov rbp, rax
mov r12, rcx
mov r13, rdx
mov r14, r8
mov r15, r9
mov qword [rsp+168], r10
call _V14large_functionv
add rbp, 1
mov rcx, r12
mov rdx, r13
mov r8, r14
mov r9, r15
mov r10, [rsp+168]
mov rax, rbp
cmp rax, r10
jl _V18scopes_single_loopxxxxxxxx_rx_L0
_V18scopes_single_loopxxxxxxxx_rx_L1:
add rcx, rdx
add rcx, r8
add rcx, r9
add rcx, [rsp+144]
add rcx, [rsp+152]
add rcx, [rsp+160]
add rcx, r10
imul rcx, rbx
imul rcx, rsi
imul rcx, rdi
mov rax, rcx
add rsp, 40
pop r15
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

global _V19scopes_nested_loopsxxxxxxxx_rx
export _V19scopes_nested_loopsxxxxxxxx_rx
_V19scopes_nested_loopsxxxxxxxx_rx:
push rbx
push rsi
push rdi
push rbp
push r12
push r13
push r14
push r15
sub rsp, 40
mov rbx, rcx
sal rbx, 1
lea rsi, [rdx*2+rdx]
lea rdi, [r8*4+r8]
xor rax, rax
mov r10, [rsp+168]
mov r11, [rsp+160]
cmp rax, r10
jge _V19scopes_nested_loopsxxxxxxxx_rx_L1
_V19scopes_nested_loopsxxxxxxxx_rx_L0:
xor rbp, rbp
cmp rbp, r11
jge _V19scopes_nested_loopsxxxxxxxx_rx_L4
_V19scopes_nested_loopsxxxxxxxx_rx_L3:
mov r12, rax
mov r13, rcx
mov r14, rdx
mov r15, r8
mov qword [rsp+136], r9
mov qword [rsp+168], r10
mov qword [rsp+160], r11
call _V14large_functionv
add rbp, 1
mov rax, r12
mov rcx, r13
mov rdx, r14
mov r8, r15
mov r9, [rsp+136]
mov r11, [rsp+160]
mov r10, [rsp+168]
cmp rbp, r11
jl _V19scopes_nested_loopsxxxxxxxx_rx_L3
_V19scopes_nested_loopsxxxxxxxx_rx_L4:
mov rbp, rax
mov r12, rcx
mov r13, rdx
mov r14, r8
mov r15, r9
mov qword [rsp+168], r10
mov qword [rsp+160], r11
call _V14large_functionv
add rbp, 1
mov rcx, r12
mov rdx, r13
mov r8, r14
mov r9, r15
mov r11, [rsp+160]
mov r10, [rsp+168]
mov rax, rbp
cmp rax, r10
jl _V19scopes_nested_loopsxxxxxxxx_rx_L0
_V19scopes_nested_loopsxxxxxxxx_rx_L1:
add rcx, rdx
add rcx, r8
add rcx, r9
add rcx, [rsp+144]
add rcx, [rsp+152]
add rcx, r11
add rcx, r10
imul rcx, rbx
imul rcx, rsi
imul rcx, rdi
mov rax, rcx
add rsp, 40
pop r15
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

_V4initv_rx:
sub rsp, 72
mov rax, 1
add rsp, 72
ret
xor rcx, rcx
xor rdx, rdx
xor r8, r8
xor r9, r9
mov qword [rsp+32], 0
mov qword [rsp+40], 0
mov qword [rsp+48], 0
mov qword [rsp+56], 0
call _V27scopes_nested_if_statementsxxxxxxxx_rx
xor rcx, rcx
xor rdx, rdx
xor r8, r8
xor r9, r9
mov qword [rsp+32], 0
mov qword [rsp+40], 0
mov qword [rsp+48], 0
mov qword [rsp+56], 0
call _V18scopes_single_loopxxxxxxxx_rx
xor rcx, rcx
xor rdx, rdx
xor r8, r8
xor r9, r9
mov qword [rsp+32], 0
mov qword [rsp+40], 0
mov qword [rsp+48], 0
mov qword [rsp+56], 0
call _V19scopes_nested_loopsxxxxxxxx_rx
ret