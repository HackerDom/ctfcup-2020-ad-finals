import requests
from gornilo import Verdict
from requests.adapters import HTTPAdapter
from requests.packages.urllib3.util.retry import Retry


def ensure_success(request):
    r = request
    if r.status_code != 200:
        raise HTTPException(Verdict.MUMBLE("Invalid status code: %s %s" % (r.url, r.status_code)))
    return r


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

        ensure_success(result)

    def login(self, name, password):
        request = {
            "name": name,
            "password": password
        }
        result = self.retryable_requests().post(
            f"http://{self.hostname}/Account/login",
            json=request, timeout=self.timeout
        )

        self.cookies = result.cookies.get_dict()

        ensure_success(result)

    # weapon
    def add_shared_weapon(self, name, is_vorpal, force, flag):
        request = {
            "name": name,
            "isVorpal": is_vorpal,
            "force": force,
            "arcaneProperty": flag
        }
        result = self.retryable_requests().put(
            f"http://{self.hostname}/Weapon/shared",
            json=request, timeout=self.timeout, cookies=self.cookies
        )

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

        ensure_success(result)

    def get_weapon(self, weapon_name):
        request = {
            "weaponName": weapon_name
        }
        result = self.retryable_requests().post(
            f"http://{self.hostname}/Weapon/weapon",
            json=request, timeout=self.timeout, cookies=self.cookies
        )

        ensure_success(result)

        return result.json()

    def get_weapon_list(self):
        result = self.retryable_requests().get(
            f"http://{self.hostname}/Weapon/weaponList",
            timeout=self.timeout, cookies=self.cookies
        )

        ensure_success(result)

        return result.json()

    # Juberwocky
    def add_juberwocky(self, breeding_seed, user, flag, force, has_jaws, has_claws):
        request = {
            "breedingSeed": breeding_seed,
            "breeder": user,
            "arcaneProperty": flag,
            "force": force,
            "hasJawsThatBite": has_jaws,
            "hasClawsThatCatch": has_claws
        }
        result = self.retryable_requests().put(
            f"http://{self.hostname}/Jabberwocky/jabberwocky",
            json=request, timeout=self.timeout, cookies=self.cookies
        )

    def get_juberwocky(self, breeding_seed):
        request = {
            "breedingSeed": breeding_seed
        }

        result = self.retryable_requests().post(
            f"http://{self.hostname}/Jabberwocky/jabberwocky",
            json=request, timeout=self.timeout, cookies=self.cookies
        )

        ensure_success(result)

        return result.json()

    def test_weapon(self, breeding_seed, weapon_name):
        request = {
            "breedingSeed": breeding_seed,
            "weaponName": weapon_name
        }

        result = self.retryable_requests().put(
            f"http://{self.hostname}/Jabberwocky/weaponTestResult",
            json=request, timeout=self.timeout, cookies=self.cookies
        )

        ensure_success(result)

        return result.json()

    def get_juberwocky_list(self):
        result = self.retryable_requests().get(
            f"http://{self.hostname}/Jabberwocky/jabberwockyList",
            timeout=self.timeout, cookies=self.cookies
        )

        ensure_success(result)

        return result.json()

    def retryable_requests(
            self,
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


class HTTPException(Exception):
    def __init__(self, verdict=None):
        self.verdict = verdict  # you could add more args

    def __str__(self):
        return str(self.verdict)
