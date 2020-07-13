section .text
global function_run
extern allocate
extern integer_power
extern sys_print
extern sys_read
extern copy
extern offset_copy
extern deallocate
extern large_function

global function_basic_for_loop
export function_basic_for_loop
function_basic_for_loop:
xor rax, rax
xor r8, r8
mov r9, rcx
cmp rax, rdx
jge function_basic_for_loop_L1
function_basic_for_loop_L0:
add r9, r8
add r8, 3
add rax, 1
cmp rax, rdx
jl function_basic_for_loop_L0
function_basic_for_loop_L1:
mov rax, r9
ret

global function_forever_loop
export function_forever_loop
function_forever_loop:
xor rax, rax
function_forever_loop_L0:
add rax, 1
jmp function_forever_loop_L0
ret

global function_conditional_loop
export function_conditional_loop
function_conditional_loop:
cmp rcx, 10
jge function_conditional_loop_L1
function_conditional_loop_L0:
add rcx, 1
cmp rcx, 10
jl function_conditional_loop_L0
function_conditional_loop_L1:
mov rax, rcx
ret

global function_conditional_action_loop
export function_conditional_action_loop
function_conditional_action_loop:
cmp rcx, 1000
jge function_conditional_action_loop_L1
function_conditional_action_loop_L0:
sal rcx, 1
cmp rcx, 1000
jl function_conditional_action_loop_L0
function_conditional_action_loop_L1:
mov rax, rcx
ret

global function_normal_for_loop
export function_normal_for_loop
function_normal_for_loop:
xor r8, r8
mov rax, rcx
cmp r8, rdx
jge function_normal_for_loop_L1
function_normal_for_loop_L0:
add rax, r8
add r8, 1
cmp r8, rdx
jl function_normal_for_loop_L0
function_normal_for_loop_L1:
ret

global function_normal_for_loop_with_stop
export function_normal_for_loop_with_stop
function_normal_for_loop_with_stop:
xor r8, r8
mov rax, rcx
cmp r8, rdx
jg function_normal_for_loop_with_stop_L1
function_normal_for_loop_with_stop_L0:
cmp r8, 100
jle function_normal_for_loop_with_stop_L2
mov rax, -1
jmp function_normal_for_loop_with_stop_L1
mov rax, -1
function_normal_for_loop_with_stop_L2:
add rax, r8
add r8, 1
cmp r8, rdx
jle function_normal_for_loop_with_stop_L0
function_normal_for_loop_with_stop_L1:
ret

global function_nested_for_loops
export function_nested_for_loops
function_nested_for_loops:
push rbx
push rsi
sub rsp, 16
xor rax, rax
xor r8, r8
cmp r8, rdx
jge function_nested_for_loops_L1
function_nested_for_loops_L0:
xor r9, r9
cmp r9, rdx
jge function_nested_for_loops_L3
function_nested_for_loops_L2:
test r9, r9
jne function_nested_for_loops_L4
add rax, 1
function_nested_for_loops_L4:
xor r10, r10
cmp r10, rdx
jge function_nested_for_loops_L7
function_nested_for_loops_L6:
mov r11, rax
mov rax, r10
mov rbx, rdx
xor rdx, rdx
mov rsi, 2
idiv rsi
test rdx, rdx
mov rdx, rbx
mov rax, r11
jne function_nested_for_loops_L9
mov r11, rax
mov rax, r9
mov rbx, rdx
xor rdx, rdx
mov rsi, 2
idiv rsi
test rdx, rdx
mov rdx, rbx
mov rax, r11
jne function_nested_for_loops_L9
mov r11, rax
mov rax, r8
mov rbx, rdx
xor rdx, rdx
mov rsi, 2
idiv rsi
test rdx, rdx
mov rdx, rbx
mov rax, r11
jne function_nested_for_loops_L9
mov r11, r8
imul r11, rdx
imul r11, rdx
mov rbx, r9
imul rbx, rdx
add r11, rbx
add r11, r10
mov byte [rcx+r11], 100
jmp function_nested_for_loops_L8
function_nested_for_loops_L9:
mov r11, r8
imul r11, rdx
imul r11, rdx
mov rbx, r9
imul rbx, rdx
add r11, rbx
add r11, r10
mov byte [rcx+r11], 0
function_nested_for_loops_L8:
test r10, r10
jne function_nested_for_loops_L13
add rax, 1
function_nested_for_loops_L13:
add r10, 1
cmp r10, rdx
jl function_nested_for_loops_L6
function_nested_for_loops_L7:
add r9, 1
cmp r9, rdx
jl function_nested_for_loops_L2
function_nested_for_loops_L3:
test r8, r8
jne function_nested_for_loops_L15
add rax, 1
function_nested_for_loops_L15:
add r8, 1
cmp r8, rdx
jl function_nested_for_loops_L0
function_nested_for_loops_L1:
add rsp, 16
pop rsi
pop rbx
ret

global function_normal_for_loop_with_memory_evacuation
export function_normal_for_loop_with_memory_evacuation
function_normal_for_loop_with_memory_evacuation:
push rbx
push rsi
push rdi
sub rsp, 48
mov rbx, rcx
mov rsi, rdx
mov rax, rbx
cmp rax, rsi
jge function_normal_for_loop_with_memory_evacuation_L1
function_normal_for_loop_with_memory_evacuation_L0:
mov rdi, rax
call large_function
add rdi, 1
cmp rdi, rsi
mov rax, rdi
jl function_normal_for_loop_with_memory_evacuation_L0
function_normal_for_loop_with_memory_evacuation_L1:
add rsp, 48
pop rdi
pop rsi
pop rbx
ret

function_run:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
xor rcx, rcx
xor rdx, rdx
call function_basic_for_loop
call function_forever_loop
xor rcx, rcx
call function_conditional_loop
xor rcx, rcx
call function_conditional_action_loop
xor rcx, rcx
xor rdx, rdx
call function_normal_for_loop
xor rcx, rcx
xor rdx, rdx
call function_normal_for_loop_with_stop
xor rcx, rcx
xor rdx, rdx
call function_nested_for_loops
xor rcx, rcx
xor rdx, rdx
call function_normal_for_loop_with_memory_evacuation
ret

section .data