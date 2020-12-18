.section .text
.intel_syntax noprefix
.global main
main:
jmp _V4initv_rx

.extern _V17internal_allocatex_rPh

.global _V14single_booleanb_rx
_V14single_booleanb_rx:
cmp rcx, 1
jne _V14single_booleanb_rx_L1
xor rax, rax
ret
jmp _V14single_booleanb_rx_L0
_V14single_booleanb_rx_L1:
mov rax, 1
ret
_V14single_booleanb_rx_L0:
ret

.global _V12two_booleansbb_rx
_V12two_booleansbb_rx:
cmp rcx, 1
jne _V12two_booleansbb_rx_L1
mov rax, 1
ret
jmp _V12two_booleansbb_rx_L0
_V12two_booleansbb_rx_L1:
cmp rdx, 1
jne _V12two_booleansbb_rx_L3
mov rax, 2
ret
jmp _V12two_booleansbb_rx_L0
_V12two_booleansbb_rx_L3:
mov rax, 3
ret
_V12two_booleansbb_rx_L0:
ret

.global _V20nested_if_statementsxxx_rx
_V20nested_if_statementsxxx_rx:
cmp rcx, 1
jne _V20nested_if_statementsxxx_rx_L1
cmp rdx, 2
jne _V20nested_if_statementsxxx_rx_L4
cmp r8, 3
jne _V20nested_if_statementsxxx_rx_L7
mov rax, 1
ret
jmp _V20nested_if_statementsxxx_rx_L6
_V20nested_if_statementsxxx_rx_L7:
cmp r8, 4
jne _V20nested_if_statementsxxx_rx_L6
mov rax, 1
ret
_V20nested_if_statementsxxx_rx_L6:
jmp _V20nested_if_statementsxxx_rx_L3
_V20nested_if_statementsxxx_rx_L4:
test rdx, rdx
jne _V20nested_if_statementsxxx_rx_L3
cmp r8, 1
jne _V20nested_if_statementsxxx_rx_L12
mov rax, 1
ret
jmp _V20nested_if_statementsxxx_rx_L11
_V20nested_if_statementsxxx_rx_L12:
cmp r8, -1
jne _V20nested_if_statementsxxx_rx_L11
mov rax, 1
ret
_V20nested_if_statementsxxx_rx_L11:
_V20nested_if_statementsxxx_rx_L3:
xor rax, rax
ret
jmp _V20nested_if_statementsxxx_rx_L0
_V20nested_if_statementsxxx_rx_L1:
cmp rcx, 2
jne _V20nested_if_statementsxxx_rx_L0
cmp rdx, 4
jne _V20nested_if_statementsxxx_rx_L17
cmp r8, 8
jne _V20nested_if_statementsxxx_rx_L20
mov rax, 1
ret
jmp _V20nested_if_statementsxxx_rx_L19
_V20nested_if_statementsxxx_rx_L20:
cmp r8, 6
jne _V20nested_if_statementsxxx_rx_L19
mov rax, 1
ret
_V20nested_if_statementsxxx_rx_L19:
jmp _V20nested_if_statementsxxx_rx_L16
_V20nested_if_statementsxxx_rx_L17:
cmp rdx, 3
jne _V20nested_if_statementsxxx_rx_L16
cmp r8, 4
jne _V20nested_if_statementsxxx_rx_L25
mov rax, 1
ret
jmp _V20nested_if_statementsxxx_rx_L24
_V20nested_if_statementsxxx_rx_L25:
cmp r8, 5
jne _V20nested_if_statementsxxx_rx_L24
mov rax, 1
ret
_V20nested_if_statementsxxx_rx_L24:
_V20nested_if_statementsxxx_rx_L16:
xor rax, rax
ret
_V20nested_if_statementsxxx_rx_L0:
xor rax, rax
ret

.global _V27logical_and_in_if_statementbb_rx
_V27logical_and_in_if_statementbb_rx:
cmp rcx, 1
jne _V27logical_and_in_if_statementbb_rx_L0
cmp rdx, 1
jne _V27logical_and_in_if_statementbb_rx_L0
mov rax, 10
ret
_V27logical_and_in_if_statementbb_rx_L0:
xor rax, rax
ret

.global _V26logical_or_in_if_statementbb_rx
_V26logical_or_in_if_statementbb_rx:
cmp rcx, 1
je _V26logical_or_in_if_statementbb_rx_L1
cmp rdx, 1
jne _V26logical_or_in_if_statementbb_rx_L0
_V26logical_or_in_if_statementbb_rx_L1:
mov rax, 10
ret
_V26logical_or_in_if_statementbb_rx_L0:
xor rax, rax
ret

.global _V25nested_logical_statementsbbbb_rx
_V25nested_logical_statementsbbbb_rx:
cmp rcx, 1
jne _V25nested_logical_statementsbbbb_rx_L1
cmp rdx, 1
jne _V25nested_logical_statementsbbbb_rx_L1
cmp r8, 1
jne _V25nested_logical_statementsbbbb_rx_L1
cmp r9, 1
jne _V25nested_logical_statementsbbbb_rx_L1
mov rax, 1
ret
jmp _V25nested_logical_statementsbbbb_rx_L0
_V25nested_logical_statementsbbbb_rx_L1:
cmp rcx, 1
je _V25nested_logical_statementsbbbb_rx_L8
cmp rdx, 1
jne _V25nested_logical_statementsbbbb_rx_L6
_V25nested_logical_statementsbbbb_rx_L8:
cmp r8, 1
jne _V25nested_logical_statementsbbbb_rx_L6
cmp r9, 1
jne _V25nested_logical_statementsbbbb_rx_L6
mov rax, 2
ret
jmp _V25nested_logical_statementsbbbb_rx_L0
_V25nested_logical_statementsbbbb_rx_L6:
cmp rcx, 1
jne _V25nested_logical_statementsbbbb_rx_L11
cmp rdx, 1
jne _V25nested_logical_statementsbbbb_rx_L11
cmp r8, 1
je _V25nested_logical_statementsbbbb_rx_L12
cmp r9, 1
jne _V25nested_logical_statementsbbbb_rx_L11
_V25nested_logical_statementsbbbb_rx_L12:
mov rax, 3
ret
jmp _V25nested_logical_statementsbbbb_rx_L0
_V25nested_logical_statementsbbbb_rx_L11:
cmp rcx, 1
jne _V25nested_logical_statementsbbbb_rx_L18
cmp rdx, 1
je _V25nested_logical_statementsbbbb_rx_L17
_V25nested_logical_statementsbbbb_rx_L18:
cmp r8, 1
jne _V25nested_logical_statementsbbbb_rx_L16
cmp r9, 1
jne _V25nested_logical_statementsbbbb_rx_L16
_V25nested_logical_statementsbbbb_rx_L17:
mov rax, 4
ret
jmp _V25nested_logical_statementsbbbb_rx_L0
_V25nested_logical_statementsbbbb_rx_L16:
cmp rcx, 1
je _V25nested_logical_statementsbbbb_rx_L22
cmp rdx, 1
je _V25nested_logical_statementsbbbb_rx_L22
cmp r8, 1
je _V25nested_logical_statementsbbbb_rx_L22
cmp r9, 1
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

.global _V19logical_operators_1xx_rx
_V19logical_operators_1xx_rx:
cmp rcx, rdx
jg _V19logical_operators_1xx_rx_L2
test rcx, rcx
jne _V19logical_operators_1xx_rx_L1
_V19logical_operators_1xx_rx_L2:
mov rax, rdx
ret
mov rdx, rax
jmp _V19logical_operators_1xx_rx_L0
_V19logical_operators_1xx_rx_L1:
cmp rcx, rdx
jne _V19logical_operators_1xx_rx_L4
cmp rdx, 1
jne _V19logical_operators_1xx_rx_L4
mov rax, rcx
ret
mov rcx, rax
jmp _V19logical_operators_1xx_rx_L0
_V19logical_operators_1xx_rx_L4:
xor rax, rax
ret
_V19logical_operators_1xx_rx_L0:
ret

.global _V19logical_operators_2xxx_rx
_V19logical_operators_2xxx_rx:
cmp rcx, rdx
jle _V19logical_operators_2xxx_rx_L3
cmp rcx, r8
jg _V19logical_operators_2xxx_rx_L2
_V19logical_operators_2xxx_rx_L3:
cmp r8, rdx
jle _V19logical_operators_2xxx_rx_L1
_V19logical_operators_2xxx_rx_L2:
mov rax, 1
ret
jmp _V19logical_operators_2xxx_rx_L0
_V19logical_operators_2xxx_rx_L1:
cmp rcx, rdx
jle _V19logical_operators_2xxx_rx_L7
cmp rdx, r8
jl _V19logical_operators_2xxx_rx_L5
_V19logical_operators_2xxx_rx_L7:
cmp r8, 1
je _V19logical_operators_2xxx_rx_L6
cmp rcx, 1
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

.global _V1fx_rx
_V1fx_rx:
cmp rcx, 7
jne _V1fx_rx_L1
mov rax, 1
ret
jmp _V1fx_rx_L0
_V1fx_rx_L1:
xor rax, rax
ret
_V1fx_rx_L0:
ret

.global _V19logical_operators_3xx_rx
_V19logical_operators_3xx_rx:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rsi, rdx
cmp rbx, 10
jg _V19logical_operators_3xx_rx_L3
mov rcx, rbx
call _V1fx_rx
cmp rax, 1
jne _V19logical_operators_3xx_rx_L1
_V19logical_operators_3xx_rx_L3:
cmp rbx, rsi
jle _V19logical_operators_3xx_rx_L1
xor rax, rax
add rsp, 40
pop rsi
pop rbx
ret
jmp _V19logical_operators_3xx_rx_L0
_V19logical_operators_3xx_rx_L1:
mov rax, 1
add rsp, 40
pop rsi
pop rbx
ret
_V19logical_operators_3xx_rx_L0:
add rsp, 40
pop rsi
pop rbx
ret

.global _V4initv_rx
_V4initv_rx:
sub rsp, 40
mov rcx, 1
mov rdx, 1
call _V19logical_operators_1xx_rx
mov rcx, 1
mov rdx, 1
mov r8, 1
call _V19logical_operators_2xxx_rx
mov rcx, 1
mov rdx, 1
call _V19logical_operators_3xx_rx
mov rcx, 1
call _V14single_booleanb_rx
mov rcx, 1
mov rdx, 1
call _V12two_booleansbb_rx
xor rcx, rcx
xor rdx, rdx
xor r8, r8
call _V20nested_if_statementsxxx_rx
mov rcx, 1
mov rdx, 1
call _V27logical_and_in_if_statementbb_rx
mov rcx, 1
mov rdx, 1
call _V26logical_or_in_if_statementbb_rx
mov rcx, 1
mov rdx, 1
mov r8, 1
mov r9, 1
call _V25nested_logical_statementsbbbb_rx
mov rax, 1
add rsp, 40
ret

.section .data

