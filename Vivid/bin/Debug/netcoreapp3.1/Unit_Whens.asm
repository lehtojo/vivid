section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh

global _V14numerical_whenx_rx
export _V14numerical_whenx_rx
_V14numerical_whenx_rx:
xor rax, rax
cmp rcx, 7
jne _V14numerical_whenx_rx_L1
imul rcx, rcx
mov rax, rcx
jmp _V14numerical_whenx_rx_L0
_V14numerical_whenx_rx_L1:
cmp rcx, 3
jne _V14numerical_whenx_rx_L3
lea rdx, [rcx+rcx]
add rdx, rcx
mov rax, rdx
jmp _V14numerical_whenx_rx_L0
_V14numerical_whenx_rx_L3:
cmp rcx, 1
jne _V14numerical_whenx_rx_L5
mov rax, -1
jmp _V14numerical_whenx_rx_L0
_V14numerical_whenx_rx_L5:
mov rdx, rcx
mov rax, rdx
_V14numerical_whenx_rx_L0:
ret

_V4initv_rx:
sub rsp, 40
xor rcx, rcx
call _V14numerical_whenx_rx
mov rax, 1
add rsp, 40
ret