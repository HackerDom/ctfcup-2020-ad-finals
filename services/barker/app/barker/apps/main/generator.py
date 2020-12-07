
def get_generator():
    state = [24432, 0]
    while True:
        n = ""
        for _ in range(32):
            s = bin(state[0] & 214748361).count("1") + state[1]
            state[0] = (state[0] >> 1) | ((s & 1) << 8)
            state[1] = s >> 1
            n += str(state[0] & 1)
        yield int(n, base=2)
