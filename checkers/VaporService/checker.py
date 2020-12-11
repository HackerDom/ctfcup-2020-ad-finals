#!/usr/bin/env python3.7
import json
from random import randrange

from gornilo import Checker, CheckRequest, PutRequest, GetRequest, Verdict
from api import Api, HTTPException
from uuid import uuid4
from requests.exceptions import RequestException
import utils

checker = Checker()


def randomize() -> str:
    return str(uuid4())


class Object(object):
    pass


@checker.define_check
def check(check_request: CheckRequest) -> Verdict:
    api = Api(check_request.hostname)
    try:
        user = Object()
        user.name = utils.get_string_range(5, 10) + utils.get_token()
        user.password = utils.get_token()
        api.register(user.name, user.password)

        shared = Object()
        shared.name = utils.get_string_range(5, 10) + utils.get_token()
        shared.isVorpal = randrange(1, 2) % 2 == 0
        shared.force = randrange(0, 100)
        shared.flag = utils.get_string_range(5, 64)
        api.add_shared_weapon(shared.name, shared.isVorpal, shared.force, shared.flag)
        checked = api.get_weapon(shared.name)
        if compare_insensetive(shared.__dict__, checked):
            return Verdict.MUMBLE("expected %s, recived %s" % (json.dumps(shared), checked))

        claimed = Object()
        claimed.name = utils.get_string_range(5, 10) + utils.get_token()
        claimed.isVorpal = randrange(0, 100) % 2 == 0
        claimed.force = randrange(0, 100)
        claimed.flag = utils.get_string_range(5, 64)
        api.add_claimed_weapon(claimed.name, claimed.isVorpal, claimed.force, claimed.flag)
        checked = api.get_weapon(claimed.name)
        if compare_insensetive(claimed.__dict__, checked):
            return Verdict.MUMBLE("expected %s, recived %s" % (json.dumps(claimed), checked))

        weapon_list = api.get_weapon_list()

        if claimed.name not in weapon_list or shared.name not in weapon_list:
            return Verdict.MUMBLE("weapon list return not all weapon")

        jw = Object()
        jw.seed = utils.get_string_range(5, 10) + utils.get_token()
        jw.breeder = user.name
        jw.flag = utils.get_string_range(5, 64)
        jw.force = randrange(0, 100)
        jw.has_jaws = randrange(1, 2) % 2 == 0
        jw.has_claws = randrange(1, 2) % 2 == 0

        api.add_juberwocky(jw.seed, jw.breeder, jw.flag, jw.force, jw.has_jaws, jw.has_claws)
        checked_jw = api.get_juberwocky(jw.seed)
        if compare_insensetive(jw.__dict__, checked_jw):
            return Verdict.MUMBLE("expected %s, recived %s" % (json.dumps(jw), checked_jw))

        test = Object()
        test.name = utils.get_string_range(5, 10) + utils.get_token()
        test.isVorpal = False
        test.force = randrange(0, 100)
        test.flag = utils.get_string_range(5, 64)
        api.add_shared_weapon(test.name, test.isVorpal, test.force, test.flag)

        result = api.test_weapon(jw.seed, test.name)
        if result['JabberwockyDefeated'] != (test.force >= jw.force):
            return Verdict.MUMBLE("Bad forecast")

        return Verdict.OK()
    except HTTPException as e:
        return e.verdict
    except RequestException as e:
        print(f"can't connect due to {e}")
        return Verdict.DOWN("can't connect to host")
    except Exception as e:
        print(e)
        return Verdict.MUMBLE("bad json")


@checker.define_put(vuln_num=1, vuln_rate=1)
def put(put_request: PutRequest) -> Verdict:
    api = Api(put_request.hostname)
    flag = put_request.flag
    try:
        user = Object()
        user.name = utils.get_user_name() + utils.get_token()
        user.password = utils.get_user_pass()
        api.register(user.name, user.password)

        claimed = Object()
        claimed.name = utils.get_string_range(5, 10) + utils.get_token()
        claimed.isVorpal = randrange(0, 100) % 2 == 0
        claimed.force = randrange(0, 100)
        claimed.flag = flag
        api.add_shared_weapon(claimed.name, claimed.isVorpal, claimed.force, claimed.flag)

        return Verdict.OK(f"{user.name}:{user.password}:{claimed.name}:{claimed.flag}")
    except HTTPException as e:
        return e.verdict
    except RequestException as e:
        print(f"timeout on connect {e}")
        return Verdict.DOWN("timeout")
    except Exception as e:
        print(f"possible mumble, {e}")
        return Verdict.MUMBLE("bad json")


@checker.define_get(vuln_num=1)
def get(get_request: GetRequest) -> Verdict:
    api = Api(get_request.hostname)
    try:
        user, password, w_name, secret = get_request.flag_id.strip().split(":")
        try:
            api.login(user, password)
            checked = api.get_weapon(w_name)
            if checked["ArcaneProperty"] == secret:
                return Verdict.OK()
            else:
                print(checked, get_request.flag)
                return Verdict.CORRUPT("bad flag")
        except Exception as e:
            print(f"bad access {e}")
            return Verdict.CORRUPT("can't reach flag")
    except HTTPException as e:
        return e.verdict
    except RequestException as e:
        print(e)
        return Verdict.DOWN("seems to be down")
    except Exception as e:
        print(f"ex {e}")
        return Verdict.MUMBLE("bad json")


@checker.define_put(vuln_num=2, vuln_rate=1)
def put2(put_request: PutRequest) -> Verdict:
    api = Api(put_request.hostname)
    flag = put_request.flag
    try:
        user = Object()
        user.name = utils.get_user_name() + utils.get_token()
        user.password = utils.get_user_pass()
        api.register(user.name, user.password)

        jw = Object()
        jw.seed = utils.get_string_range(5, 10) + utils.get_token()
        jw.breeder = user.name
        jw.flag = flag
        jw.force = randrange(0, 100)
        jw.has_jaws = randrange(1, 2) % 2 == 0
        jw.has_claws = randrange(1, 2) % 2 == 0

        api.add_juberwocky(jw.seed, jw.breeder, jw.flag, jw.force, jw.has_jaws, jw.has_claws)

        return Verdict.OK(f"{user.name}:{user.password}:{jw.seed}:{jw.flag}")
    except HTTPException as e:
        return e.verdict
    except RequestException as e:
        print(f"timeout on connect {e}")
        return Verdict.DOWN("timeout")
    except Exception as e:
        print(f"possible mumble, {e}")
        return Verdict.MUMBLE("bad json")


@checker.define_get(vuln_num=2)
def get2(get_request: GetRequest) -> Verdict:
    api = Api(get_request.hostname)
    try:
        user, password, seed, secret = get_request.flag_id.strip().split(":")
        try:
            api.login(user, password)
            checked_jw = api.get_juberwocky(seed)
            if checked_jw["ArcaneProperty"] == secret:
                return Verdict.OK()
            else:
                print(checked_jw, get_request.flag)
                return Verdict.CORRUPT("bad flag")
        except Exception as e:
            print(f"bad access {e}")
            return Verdict.CORRUPT("can't reach flag")
    except HTTPException as e:
        return e.verdict
    except RequestException as e:
        print(e)
        return Verdict.DOWN("seems to be down")
    except Exception as e:
        print(f"ex {e}")
        return Verdict.MUMBLE("bad json")


def compare_insensetive(first, second):
    first_str = json.dumps(first).lower()
    second_str = json.dumps(second).lower()
    return first_str == second_str


checker.run()
