import random
import uuid
import string
from random import randrange

UserAgents = None
Names = None


def get_user_agent():
    global UserAgents
    if UserAgents is None:
        with open('user-agents') as fin:
            UserAgents = [line.strip() for line in fin]
    return random.choice(UserAgents)


def get_user_name():
    return get_random_string(randrange(5, 30))


def get_user_pass():
    return get_random_string(randrange(5, 30))


def get_command_name():
    return get_random_string(randrange(5, 30))


def get_victim_name():
    return get_random_string(randrange(5, 10))


def get_token():
    return str(uuid.uuid4())


def get_random_string(min_len, max_len):
    return get_random_string(randrange(min_len, max_len))


def get_random_string(N):
    return ''.join(random.choice(string.ascii_uppercase + string.digits) for _ in range(N))
