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
sub rsp, 48
mov rbx, rcx
sal rbx, 1
lea rsi, [rdx*2+rdx]
lea rdi, [r8*4+r8]
test rcx, rcx
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L1
test r8, r8
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L3
mov rbp, rcx
mov r12, rdx
mov r13, r8
mov r14, r9
call _V14large_functionv
mov rcx, rbp
mov rdx, r12
mov r8, r13
mov r9, r14
_V27scopes_nested_if_statementsxxxxxxxx_rx_L3:
mov rbp, rcx
mov r12, rdx
mov r13, r8
mov r14, r9
call _V14large_functionv
mov rcx, rbp
mov rdx, r12
mov r8, r13
mov r9, r14
jmp _V27scopes_nested_if_statementsxxxxxxxx_rx_L0
_V27scopes_nested_if_statementsxxxxxxxx_rx_L1:
test rdx, rdx
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L5
test r9, r9
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L7
mov rbp, rcx
mov r12, rdx
mov r13, r8
mov r14, r9
call _V14large_functionv
mov rcx, rbp
mov rdx, r12
mov r8, r13
mov r9, r14
_V27scopes_nested_if_statementsxxxxxxxx_rx_L7:
mov rbp, rcx
mov r12, rdx
mov r13, r8
mov r14, r9
call _V14large_functionv
mov rcx, rbp
mov rdx, r12
mov r8, r13
mov r9, r14
jmp _V27scopes_nested_if_statementsxxxxxxxx_rx_L0
_V27scopes_nested_if_statementsxxxxxxxx_rx_L5:
mov r10, [rsp+144]
test r10, r10
mov qword [rsp+144], r10
jle _V27scopes_nested_if_statementsxxxxxxxx_rx_L9
mov rbp, rcx
mov r12, rdx
mov r13, r8
mov r14, r9
call _V14large_functionv
mov rcx, rbp
mov rdx, r12
mov r8, r13
mov r9, r14
_V27scopes_nested_if_statementsxxxxxxxx_rx_L9:
mov rbp, rcx
mov r12, rdx
mov r13, r8
mov r14, r9
call _V14large_functionv
mov rcx, rbp
mov rdx, r12
mov r8, r13
mov r9, r14
_V27scopes_nested_if_statementsxxxxxxxx_rx_L0:
add rcx, rdx
add rcx, r8
add rcx, r9
add rcx, [rsp+144]
add rcx, [rsp+152]
add rcx, [rsp+160]
add rcx, [rsp+168]
imul rcx, rbx
imul rcx, rsi
imul rcx, rdi
mov rax, rcx
add rsp, 48
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
xor rbp, rbp
mov r12, [rsp+168]
cmp rbp, r12
jge _V18scopes_single_loopxxxxxxxx_rx_L1
_V18scopes_single_loopxxxxxxxx_rx_L0:
mov r13, rcx
mov r14, rdx
mov r15, r8
mov qword [rsp+136], r9
call _V14large_functionv
add rbp, 1
mov rcx, r13
mov rdx, r14
mov r8, r15
mov r9, [rsp+136]
cmp rbp, r12
jl _V18scopes_single_loopxxxxxxxx_rx_L0
_V18scopes_single_loopxxxxxxxx_rx_L1:
add rcx, rdx
add rcx, r8
add rcx, r9
add rcx, [rsp+144]
add rcx, [rsp+152]
add rcx, [rsp+160]
add rcx, r12
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
xor rbp, rbp
mov r12, [rsp+168]
mov r13, [rsp+160]
cmp rbp, r12
jge _V19scopes_nested_loopsxxxxxxxx_rx_L1
_V19scopes_nested_loopsxxxxxxxx_rx_L0:
xor r14, r14
cmp r14, r13
jge _V19scopes_nested_loopsxxxxxxxx_rx_L4
_V19scopes_nested_loopsxxxxxxxx_rx_L3:
mov r15, rcx
mov qword [rsp+120], rdx
mov qword [rsp+128], r8
mov qword [rsp+136], r9
call _V14large_functionv
add r14, 1
mov rcx, r15
mov rdx, [rsp+120]
mov r8, [rsp+128]
mov r9, [rsp+136]
cmp r14, r13
jl _V19scopes_nested_loopsxxxxxxxx_rx_L3
_V19scopes_nested_loopsxxxxxxxx_rx_L4:
mov r14, rcx
mov r15, rdx
mov qword [rsp+128], r8
mov qword [rsp+136], r9
call _V14large_functionv
add rbp, 1
mov rcx, r14
mov rdx, r15
mov r8, [rsp+128]
mov r9, [rsp+136]
cmp rbp, r12
jl _V19scopes_nested_loopsxxxxxxxx_rx_L0
_V19scopes_nested_loopsxxxxxxxx_rx_L1:
add rcx, rdx
add rcx, r8
add rcx, r9
add rcx, [rsp+144]
add rcx, [rsp+152]
add rcx, r13
add rcx, r12
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