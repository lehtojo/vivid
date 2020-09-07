section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh

_V1gx_rx:
add rdi, 1
mov rax, rdi
ret

_V1fx_rx:
lea rcx, [rdi+1]
imul rdi, rcx
mov rax, rdi
ret

_V1hx_rx:
sal rdi, 1
mov rax, rdi
ret

_V4initv_rx:
sub rsp, 8
call _V15inlines_membersv_rx
mov rdi, 1
mov rsi, 2
call _V30inlines_conditional_statementsxx_rx
lea rcx, [7+1]
mov rdx, 7
mov rsi, rdx
imul rsi, rcx
xor rax, rax
xor rdi, rdi
cmp rdi, 10
jge _V4initv_rx_L1
_V4initv_rx_L0:
add rax, rsi
add rdi, 1
cmp rdi, 10
jl _V4initv_rx_L0
_V4initv_rx_L1:
add rsp, 8
ret

_V15inlines_membersv_rx:
sub rsp, 8
mov rdi, 1
mov rsi, 2
call _VN4Type4initExx_rPh
mov rcx, [rax]
add rcx, [rax+8]
mov rdx, [rax]
add rdx, [rax+8]
mov rax, [rax]
add rax, [rax+8]
add rax, rcx
sal rdx, 1
sub rax, rdx
add rsp, 8
ret

_V30inlines_conditional_statementsxx_rx:
lea rcx, [rdi+rsi]
lea rdx, [rcx+1]
mov r8, rcx
imul r8, rdx
mov r9, rdi
imul r9, rsi
add r9, 1
cmp r8, r9
jle _V30inlines_conditional_statementsxx_rx_L1
lea rcx, [rdi+1]
mov rax, rdi
imul rax, rcx
lea rcx, [rax+1]
imul rax, rcx
ret
jmp _V30inlines_conditional_statementsxx_rx_L0
_V30inlines_conditional_statementsxx_rx_L1:
lea rcx, [rdi+1]
mov rdx, rsi
sal rdx, 1
lea r8, [rdi+1]
mov r9, rdi
imul r9, r8
mov r8, rdx
imul r8, r9
cmp rcx, r8
jle _V30inlines_conditional_statementsxx_rx_L0
lea rcx, [rdi+1]
mov rdx, rdi
imul rdx, rcx
mov rcx, rsi
sal rcx, 1
add rdx, rcx
lea rax, [rdx+1]
ret
_V30inlines_conditional_statementsxx_rx_L0:
add rdi, rsi
mov rax, rdi
ret

_VN4Type4initExx_rPh:
push rbx
push rbp
sub rsp, 8
mov rcx, rdi
mov rdi, 16
mov rbx, rcx
mov rbp, rsi
call _V8allocatex_rPh
mov qword [rax], rbx
mov qword [rax+8], rbp
add rsp, 8
pop rbp
pop rbx
ret

_VN4Type7get_sumEv_rx:
mov rax, [rdi]
add rax, [rdi+8]
ret