section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh

global _V14single_booleanb_rx
_V14single_booleanb_rx:
movzx rdi, dil
cmp rdi, 1
jne _V14single_booleanb_rx_L1
xor rax, rax
ret
jmp _V14single_booleanb_rx_L0
_V14single_booleanb_rx_L1:
mov rax, 1
ret
_V14single_booleanb_rx_L0:
ret

global _V12two_booleansbb_rx
_V12two_booleansbb_rx:
movzx rdi, dil
cmp rdi, 1
jne _V12two_booleansbb_rx_L1
mov rax, 1
ret
jmp _V12two_booleansbb_rx_L0
_V12two_booleansbb_rx_L1:
movzx rsi, sil
cmp rsi, 1
jne _V12two_booleansbb_rx_L3
mov rax, 2
ret
jmp _V12two_booleansbb_rx_L0
_V12two_booleansbb_rx_L3:
mov rax, 3
ret
_V12two_booleansbb_rx_L0:
ret

global _V20nested_if_statementsxxx_rx
_V20nested_if_statementsxxx_rx:
cmp rdi, 1
jne _V20nested_if_statementsxxx_rx_L0
cmp rsi, 2
jne _V20nested_if_statementsxxx_rx_L2
cmp rdx, 3
jne _V20nested_if_statementsxxx_rx_L5
mov rax, 1
ret
jmp _V20nested_if_statementsxxx_rx_L4
_V20nested_if_statementsxxx_rx_L5:
cmp rdx, 4
jne _V20nested_if_statementsxxx_rx_L4
mov rax, 1
ret
_V20nested_if_statementsxxx_rx_L4:
_V20nested_if_statementsxxx_rx_L2:
test rsi, rsi
jne _V20nested_if_statementsxxx_rx_L8
cmp rdx, 1
jne _V20nested_if_statementsxxx_rx_L11
mov rax, 1
ret
jmp _V20nested_if_statementsxxx_rx_L10
_V20nested_if_statementsxxx_rx_L11:
mov rcx, -1
cmp rdx, rcx
jne _V20nested_if_statementsxxx_rx_L10
mov rax, 1
ret
_V20nested_if_statementsxxx_rx_L10:
_V20nested_if_statementsxxx_rx_L8:
xor rax, rax
ret
_V20nested_if_statementsxxx_rx_L0:
cmp rdi, 2
jne _V20nested_if_statementsxxx_rx_L14
cmp rsi, 4
jne _V20nested_if_statementsxxx_rx_L16
cmp rdx, 8
jne _V20nested_if_statementsxxx_rx_L19
mov rax, 1
ret
jmp _V20nested_if_statementsxxx_rx_L18
_V20nested_if_statementsxxx_rx_L19:
cmp rdx, 6
jne _V20nested_if_statementsxxx_rx_L18
mov rax, 1
ret
_V20nested_if_statementsxxx_rx_L18:
_V20nested_if_statementsxxx_rx_L16:
cmp rsi, 3
jne _V20nested_if_statementsxxx_rx_L22
cmp rdx, 4
jne _V20nested_if_statementsxxx_rx_L25
mov rax, 1
ret
jmp _V20nested_if_statementsxxx_rx_L24
_V20nested_if_statementsxxx_rx_L25:
cmp rdx, 5
jne _V20nested_if_statementsxxx_rx_L24
mov rax, 1
ret
_V20nested_if_statementsxxx_rx_L24:
_V20nested_if_statementsxxx_rx_L22:
xor rax, rax
ret
_V20nested_if_statementsxxx_rx_L14:
xor rax, rax
ret

global _V27logical_and_in_if_statementbb_rx
_V27logical_and_in_if_statementbb_rx:
movzx rdi, dil
cmp rdi, 1
jne _V27logical_and_in_if_statementbb_rx_L0
movzx rsi, sil
cmp rsi, 1
jne _V27logical_and_in_if_statementbb_rx_L0
mov rax, 10
ret
_V27logical_and_in_if_statementbb_rx_L0:
xor rax, rax
ret

global _V26logical_or_in_if_statementbb_rx
_V26logical_or_in_if_statementbb_rx:
movzx rdi, dil
cmp rdi, 1
je _V26logical_or_in_if_statementbb_rx_L1
movzx rsi, sil
cmp rsi, 1
jne _V26logical_or_in_if_statementbb_rx_L0
_V26logical_or_in_if_statementbb_rx_L1:
mov rax, 10
ret
_V26logical_or_in_if_statementbb_rx_L0:
xor rax, rax
ret

global _V25nested_logical_statementsbbbb_rx
_V25nested_logical_statementsbbbb_rx:
movzx rdi, dil
cmp rdi, 1
jne _V25nested_logical_statementsbbbb_rx_L1
movzx rsi, sil
cmp rsi, 1
jne _V25nested_logical_statementsbbbb_rx_L1
movzx rdx, dl
cmp rdx, 1
jne _V25nested_logical_statementsbbbb_rx_L1
movzx rcx, cl
cmp rcx, 1
jne _V25nested_logical_statementsbbbb_rx_L1
mov rax, 1
ret
jmp _V25nested_logical_statementsbbbb_rx_L0
_V25nested_logical_statementsbbbb_rx_L1:
movzx rdi, dil
cmp rdi, 1
je _V25nested_logical_statementsbbbb_rx_L8
movzx rsi, sil
cmp rsi, 1
jne _V25nested_logical_statementsbbbb_rx_L6
_V25nested_logical_statementsbbbb_rx_L8:
movzx rdx, dl
cmp rdx, 1
jne _V25nested_logical_statementsbbbb_rx_L6
movzx rcx, cl
cmp rcx, 1
jne _V25nested_logical_statementsbbbb_rx_L6
mov rax, 2
ret
jmp _V25nested_logical_statementsbbbb_rx_L0
_V25nested_logical_statementsbbbb_rx_L6:
movzx rdi, dil
cmp rdi, 1
jne _V25nested_logical_statementsbbbb_rx_L11
movzx rsi, sil
cmp rsi, 1
jne _V25nested_logical_statementsbbbb_rx_L11
movzx rdx, dl
cmp rdx, 1
je _V25nested_logical_statementsbbbb_rx_L12
movzx rcx, cl
cmp rcx, 1
jne _V25nested_logical_statementsbbbb_rx_L11
_V25nested_logical_statementsbbbb_rx_L12:
mov rax, 3
ret
jmp _V25nested_logical_statementsbbbb_rx_L0
_V25nested_logical_statementsbbbb_rx_L11:
movzx rdi, dil
cmp rdi, 1
jne _V25nested_logical_statementsbbbb_rx_L18
movzx rsi, sil
cmp rsi, 1
je _V25nested_logical_statementsbbbb_rx_L17
_V25nested_logical_statementsbbbb_rx_L18:
movzx rdx, dl
cmp rdx, 1
jne _V25nested_logical_statementsbbbb_rx_L16
movzx rcx, cl
cmp rcx, 1
jne _V25nested_logical_statementsbbbb_rx_L16
_V25nested_logical_statementsbbbb_rx_L17:
mov rax, 4
ret
jmp _V25nested_logical_statementsbbbb_rx_L0
_V25nested_logical_statementsbbbb_rx_L16:
movzx rdi, dil
cmp rdi, 1
je _V25nested_logical_statementsbbbb_rx_L22
movzx rsi, sil
cmp rsi, 1
je _V25nested_logical_statementsbbbb_rx_L22
movzx rdx, dl
cmp rdx, 1
je _V25nested_logical_statementsbbbb_rx_L22
movzx rcx, cl
cmp rcx, 1
jne _V25nested_logical_statementsbbbb_rx_L21
_V25nested_logical_statementsbbbb_rx_L22:
mov rax, 5
ret
jmp _V25nested_logical_statementsbbbb_rx_L0
_V25nested_logical_statementsbbbb_rx_L21:
mov rax, 6
ret
_V25nested_logical_statementsbbbb_rx_L0:
ret

global _V19logical_operators_1xx_rx
_V19logical_operators_1xx_rx:
cmp rdi, rsi
jg _V19logical_operators_1xx_rx_L2
test rdi, rdi
jne _V19logical_operators_1xx_rx_L1
_V19logical_operators_1xx_rx_L2:
mov rax, rsi
ret
jmp _V19logical_operators_1xx_rx_L0
_V19logical_operators_1xx_rx_L1:
cmp rdi, rsi
jne _V19logical_operators_1xx_rx_L4
cmp rsi, 1
jne _V19logical_operators_1xx_rx_L4
mov rax, rdi
ret
jmp _V19logical_operators_1xx_rx_L0
_V19logical_operators_1xx_rx_L4:
xor rax, rax
ret
_V19logical_operators_1xx_rx_L0:
ret

global _V19logical_operators_2xxx_rx
_V19logical_operators_2xxx_rx:
cmp rdi, rsi
jle _V19logical_operators_2xxx_rx_L3
cmp rdi, rdx
jg _V19logical_operators_2xxx_rx_L2
_V19logical_operators_2xxx_rx_L3:
cmp rdx, rsi
jle _V19logical_operators_2xxx_rx_L1
_V19logical_operators_2xxx_rx_L2:
mov rax, 1
ret
jmp _V19logical_operators_2xxx_rx_L0
_V19logical_operators_2xxx_rx_L1:
cmp rdi, rsi
jle _V19logical_operators_2xxx_rx_L7
cmp rsi, rdx
jl _V19logical_operators_2xxx_rx_L5
_V19logical_operators_2xxx_rx_L7:
cmp rdx, 1
je _V19logical_operators_2xxx_rx_L6
cmp rdi, 1
jne _V19logical_operators_2xxx_rx_L5
_V19logical_operators_2xxx_rx_L6:
xor rax, rax
ret
jmp _V19logical_operators_2xxx_rx_L0
_V19logical_operators_2xxx_rx_L5:
mov rax, -1
ret
_V19logical_operators_2xxx_rx_L0:
ret

_V1fx_rx:
cmp rdi, 7
jne _V1fx_rx_L1
mov rax, 1
ret
jmp _V1fx_rx_L0
_V1fx_rx_L1:
xor rax, rax
ret
_V1fx_rx_L0:
ret

global _V19logical_operators_3xx_rx
_V19logical_operators_3xx_rx:
push rbx
push rbp
sub rsp, 8
cmp rdi, 10
jg _V19logical_operators_3xx_rx_L3
mov rbx, rsi
mov rbp, rdi
call _V1fx_rx
cmp rax, 1
mov rdi, rbp
mov rsi, rbx
jne _V19logical_operators_3xx_rx_L1
_V19logical_operators_3xx_rx_L3:
cmp rdi, rsi
jle _V19logical_operators_3xx_rx_L1
xor rax, rax
add rsp, 8
pop rbp
pop rbx
ret
jmp _V19logical_operators_3xx_rx_L0
_V19logical_operators_3xx_rx_L1:
mov rax, 1
add rsp, 8
pop rbp
pop rbx
ret
_V19logical_operators_3xx_rx_L0:
add rsp, 8
pop rbp
pop rbx
ret

_V4initv_rx:
sub rsp, 8
mov rdi, 1
mov rsi, 1
call _V19logical_operators_1xx_rx
mov rdi, 1
mov rsi, 1
mov rdx, 1
call _V19logical_operators_2xxx_rx
mov rdi, 1
mov rsi, 1
call _V19logical_operators_3xx_rx
mov rdi, 1
call _V14single_booleanb_rx
mov rdi, 1
mov rsi, 1
call _V12two_booleansbb_rx
xor rdi, rdi
xor rsi, rsi
xor rdx, rdx
call _V20nested_if_statementsxxx_rx
mov rdi, 1
mov rsi, 1
call _V27logical_and_in_if_statementbb_rx
mov rdi, 1
mov rsi, 1
call _V26logical_or_in_if_statementbb_rx
mov rdi, 1
mov rsi, 1
mov rdx, 1
mov rcx, 1
call _V25nested_logical_statementsbbbb_rx
mov rax, 1
add rsp, 8
ret