from django.apps import AppConfig
from .generator import get_generator


class MainConfig(AppConfig):
    name = 'main'
    generator = get_generator()
