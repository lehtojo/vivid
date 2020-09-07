section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh
extern _V14large_functionv

global _V17reference_decoy_1x_rx
_V17reference_decoy_1x_rx:
mov rcx, rdi
mov rcx, 1
add rcx, rdi
add rdi, rcx
mov rax, rdi
ret

global _V17reference_decoy_2x_rx
_V17reference_decoy_2x_rx:
sal rdi, 1
mov rcx, 1
sal rcx, 1
add rdi, rcx
mov rax, rdi
ret

global _V17reference_decoy_3x_rx
_V17reference_decoy_3x_rx:
xor rax, rax
mov rcx, rdi
mov rdx, rdi
mov rsi, rax
cmp rsi, 3
jge _V17reference_decoy_3x_rx_L1
_V17reference_decoy_3x_rx_L0:
lea rcx, [rsi+1]
mov rdx, rsi
add rdx, 1
cmp rdx, 3
mov r8, rsi
mov rsi, rdx
mov rdx, rcx
mov rcx, r8
jl _V17reference_decoy_3x_rx_L0
_V17reference_decoy_3x_rx_L1:
add rcx, rdx
mov rax, rcx
ret

global _V17reference_decoy_4x_rx
_V17reference_decoy_4x_rx:
xor rax, rax
mov rcx, rdi
mov rdx, rdi
mov rsi, rdi
mov r8, rdi
cmp rax, 5
jge _V17reference_decoy_4x_rx_L1
_V17reference_decoy_4x_rx_L0:
add rcx, 1
add rdx, 2
add rsi, 4
add r8, 8
add rax, 1
cmp rax, 5
jl _V17reference_decoy_4x_rx_L0
_V17reference_decoy_4x_rx_L1:
add rcx, rdx
add rcx, rsi
add rcx, r8
mov rax, rcx
ret

global _V17reference_decoy_5x_rx
_V17reference_decoy_5x_rx:
push rbx
push rbp
push r12
sub rsp, 16
mov rbx, rdi
call _V14large_functionv
xor rax, rax
mov rcx, rbx
mov rdx, rbx
mov rsi, rbx
mov rdi, rbx
mov r8, rbx
mov r9, rbx
mov r10, rbx
mov r11, rbx
mov rbp, rbx
mov r12, rbx
cmp rax, 5
jge _V17reference_decoy_5x_rx_L1
_V17reference_decoy_5x_rx_L0:
add rcx, 1
add rdx, 2
add rsi, 3
add rdi, 4
add r8, 5
add r9, 6
add r10, 7
add r11, 8
add rbp, 9
add r12, 10
add rax, 1
cmp rax, 5
jl _V17reference_decoy_5x_rx_L0
_V17reference_decoy_5x_rx_L1:
add rcx, rdx
add rcx, rsi
add rcx, rdi
add rcx, r8
add rcx, r9
add rcx, r10
add rcx, r11
add rcx, rbp
add rcx, r12
mov rax, rcx
add rsp, 16
pop r12
pop rbp
pop rbx
ret

_V4initv_rx:
sub rsp, 8
mov rax, 1
add rsp, 8
ret
mov rdi, 10
call _V17reference_decoy_1x_rx
mov rdi, 10
call _V17reference_decoy_2x_rx
mov rdi, 10
call _V17reference_decoy_3x_rx
mov rdi, 10
call _V17reference_decoy_4x_rx
mov rdi, 10
call _V17reference_decoy_5x_rx
ret