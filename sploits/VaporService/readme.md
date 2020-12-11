Author: [@capitanbanana](https://github.com/capitanbanana)
Сервис это апишка написанная на asp.netcore, которая умеет предсказывать исход битвы с бармаглотом. Флаги хранятся в описании бармаглотов и оружия.

# Vuln1.
Оружие бывает двух типов claimed - может получить только пользователь который его и загрузил и shared кто угодно. Знание о том, что оружие кому-то принадлежит хранятся в ClaimsIndex, а его описание в файлике. И индекс и файлики периодически чистятся от устаревших записей, но в коде который чистить файлики есть ошибка:
```
if (File.GetCreationTimeUtc(file) - DateTime.UtcNow >= settingsProvider.StorageSettings.WeaponTTL)
	File.Delete(file);
```
Поэтому записи из индекса пропадают, а файлики остаются и после очистки индекса их может читать кто угодно.

# Vuln2.
В кофигурации сервиса кто-то забыл убрать app.UseDeveloperExceptionPage(), поэтому любое выброшенное исключение будет прокинуто наружу вместо с 500-кой. В коде FightForecaster-а, бросается исключение, из которого можно получить jabberwocky.ArcaneProperty, в котором и лежит флаг. Посмотрим на код:
```
if (weapon.IsVorpal)
	return ...

if (weapon.Force > jabberwocky.Force)
	return ...

if (weapon.Force < jabberwocky.Force)
	return ...

if (Math.Abs(weapon.Force - jabberwocky.Force) < Double.Epsilon)
	return ...

throw new ArgumentException("Unpredictable jabberwocky or weapon", $"{jabberwocky} {weapon}");
```

Обычно число не может быть не больше, не меньше и не равно какому-то другому числу, но double.NaN ведет себя именно так и если weapon.Force = NaN, то исключение будет выброшено



